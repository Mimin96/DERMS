using CalculationEngineServiceCommon;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class CEUpdateThroughUIService : ICEUpdateThroughUI
    {
        public async Task<float> UpdateThroughUI(long data)
        {
            float energyFromSource = PopulateBalance(data).Result;
            return energyFromSource;
        }

        public async Task<float> PopulateBalance(long gid)
        {
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );
            Dictionary<long, DerForecastDayAhead> productionCachedDictionary = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetDerForecasts()).Result;
            Dictionary<long, IdentifiedObject> nmsCacheDictionary = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            Dictionary<int, List<long>> temp = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTurnedOffGenerators()).Result;
            List<long> turnedOffGeneratorsList = new List<long>();
            if (temp.Count > 0)
                turnedOffGeneratorsList = temp[0];

            float energyFromSource = Balance(productionCachedDictionary, gid, nmsCacheDictionary, turnedOffGeneratorsList).Result;
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead()).Wait();

            return energyFromSource;
        }

        public async Task<float> Balance(Dictionary<long, DerForecastDayAhead> prod, long GidUi, Dictionary<long, IdentifiedObject> networkModel, List<long> TurnedOffGenerators)
        {

            Dictionary<long, double> battery = new Dictionary<long, double>();
            Dictionary<long, List<long>> energySources = new Dictionary<long, List<long>>();

            Dictionary<long, DerForecastDayAhead> dicGener = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DerForecastDayAhead> tempDiffrence = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, double> dicForScada = new Dictionary<long, double>();
            List<long> changeFlexOfGen = new List<long>();
            float energyFromSource = 0;
            IdentifiedObject io = networkModel[GidUi];
            var type = io.GetType();

            if (type.Name.Equals("GeographicalRegion"))
            {
                List<long> temp = new List<long>();
                GeographicalRegion geographicalRegion = (GeographicalRegion)io;
                battery.Add(GidUi, 0);
                foreach (var item in geographicalRegion.Regions)
                {
                    battery.Add(item, 0);
                    SubGeographicalRegion subgeo = (SubGeographicalRegion)networkModel[item];
                    foreach (var sub in subgeo.Substations)
                    {
                        battery.Add(sub, 0);
                        Substation substation = (Substation)networkModel[sub];

                        foreach (long es in substation.Equipments)
                        {
                            IdentifiedObject sourceio = networkModel[es];
                            var source = sourceio.GetType();
                            if (source.Name.Equals("EnergySource"))
                            {
                                if (!temp.Contains(es))
                                    temp.Add(es);
                            }

                        }

                    }
                }
                if (temp.Count != 0)
                {
                    energySources.Add(GidUi, temp);
                }

            }
            else if (type.Name.Equals("SubGeographicalRegion"))
            {
                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)io;
                battery.Add(GidUi, 0);
                List<long> temp = new List<long>();
                foreach (var sub in subGeographicalRegion.Substations)
                {
                    battery.Add(sub, 0);
                    Substation substation = (Substation)networkModel[sub];
                    foreach (long es in substation.Equipments)
                    {
                        IdentifiedObject sourceio = networkModel[es];
                        var source = sourceio.GetType();
                        if (source.Name.Equals("EnergySource"))
                        {
                            if (!temp.Contains(es))
                                temp.Add(es);
                        }

                    }
                }
                if (temp.Count != 0)
                {
                    energySources.Add(GidUi, temp);
                }
            }
            else if (type.Name.Equals("Substation"))
            {
                battery.Add(GidUi, 0);
                List<long> temp = new List<long>();
                Substation substation = (Substation)networkModel[GidUi];
                foreach (long es in substation.Equipments)
                {
                    IdentifiedObject sourceio = networkModel[es];
                    var source = sourceio.GetType();
                    if (source.Name.Equals("EnergySource"))
                    {
                        if (!temp.Contains(es))
                            temp.Add(es);
                    }

                }
                if (temp.Count != 0)
                {
                    energySources.Add(GidUi, temp);
                }
            }

            foreach (KeyValuePair<long, double> kvp in battery)
            {
                DerForecastDayAhead derForecastDayAhead = prod[kvp.Key];

                foreach (HourDataPoint hdpProduction in derForecastDayAhead.Production.Hourly)
                {
                    foreach (HourDataPoint hdpConsumption in derForecastDayAhead.Consumption.Hourly)
                    {

                        if (hdpConsumption.Time.Equals(hdpProduction.Time))
                        {

                            if (hdpProduction.ActivePower > hdpConsumption.ActivePower)
                            {

                                //hdpProduction.ActivePower = hdpConsumption.ActivePower;
                                //PROBAJ DA SMANJIS FLEX KOLIKO MOZE -> AKO JE DOVOLJNO DA SE PORAVNA
                                // NE DAJES NISTA SORSU, AKO OPET PROIZVODI VISE -> OSTATAK PREDAJ SORSU
                                var it = networkModel[kvp.Key];
                                var tip = it.GetType();

                                if (tip.Name.Equals("Substation"))
                                {
                                    Substation sub = (Substation)it;
                                    float possibleIncrease = 0;
                                    int numberOfGen = 0;
                                    Dictionary<long, float> dicIncreaseValues = new Dictionary<long, float>();
                                    foreach (var item in sub.Equipments)
                                    {
                                        var generatorType = networkModel[item].GetType();
                                        if (generatorType.Name.Equals("Generator"))
                                        {
                                            if (!TurnedOffGenerators.Contains(item))
                                            {
                                                Generator generator = (Generator)networkModel[item];
                                                DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                {
                                                    if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                    {
                                                        possibleIncrease += generatorProduction.ActivePower * (generator.MinFlexibility / 100);
                                                        dicIncreaseValues.Add(generator.GlobalId, generatorProduction.ActivePower * (generator.MinFlexibility / 100));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    Dictionary<long, float> tempDicIncreaseValues = new Dictionary<long, float>();
                                    Dictionary<long, float> tempDicProcentValues = new Dictionary<long, float>();
                                    if (possibleIncrease >= hdpProduction.ActivePower - hdpConsumption.ActivePower)
                                    {
                                        foreach (var item in dicIncreaseValues)
                                        {
                                            float x = item.Value / (possibleIncrease / (hdpProduction.ActivePower - hdpConsumption.ActivePower));
                                            tempDicIncreaseValues.Add(item.Key, x);
                                        }
                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];
                                                    DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                    foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                    {
                                                        if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                        {
                                                            float percentValue = 0;
                                                            ///POgledaja ovo
                                                            percentValue = (tempDicIncreaseValues[generator.GlobalId] * 100) / generatorProduction.ActivePower;
                                                            tempDicProcentValues.Add(generator.GlobalId, percentValue);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];
                                                    if (generator.Flexibility)
                                                    {
                                                        DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                        foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                        {
                                                            if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                            {
                                                                if (generator.GeneratorType.Equals(GeneratorType.Solar))
                                                                {
                                                                    if (generatorProduction.ActivePower != 0)
                                                                        generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 - (tempDicProcentValues[generator.GlobalId] / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generator.GeneratorType.Equals(GeneratorType.Wind))
                                                                {
                                                                    if (generatorProduction.ActivePower != 0)
                                                                        generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 - (tempDicProcentValues[generator.GlobalId] / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }

                                                                if (generatorProduction.Time.Hour.Equals(DateTime.Now.Hour))
                                                                {
                                                                    dicForScada.Add(generator.GlobalId, -tempDicProcentValues[generator.GlobalId]);
                                                                }
                                                            }
                                                        }
                                                        if (!changeFlexOfGen.Contains(generator.GlobalId))
                                                        {
                                                            changeFlexOfGen.Add(generator.GlobalId);
                                                        }

                                                    }
                                                }
                                            }

                                        }
                                    }

                                    else
                                    {
                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];
                                                    if (generator.Flexibility)
                                                    {
                                                        DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                        foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                        {
                                                            if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                            {
                                                                if (generator.GeneratorType.Equals(GeneratorType.Solar))
                                                                {
                                                                    generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 - (generator.MinFlexibility / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generator.GeneratorType.Equals(GeneratorType.Wind))
                                                                {
                                                                    generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 - (generator.MinFlexibility / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generatorProduction.Time.Hour.Equals(DateTime.Now.Hour))
                                                                {
                                                                    dicForScada.Add(generator.GlobalId, -generator.MinFlexibility);
                                                                }


                                                            }
                                                        }
                                                        if (!changeFlexOfGen.Contains(generator.GlobalId))
                                                        {
                                                            changeFlexOfGen.Add(generator.GlobalId);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }


                            }
                            else if (hdpProduction.ActivePower <= hdpConsumption.ActivePower)
                            {
                                //POKUSAJ SA FLEX DA PODIGNES-> AKO NEMA DOSTA OSTATAK POVUCI IZ SOURSA
                                var it = networkModel[kvp.Key];
                                var tip = it.GetType();

                                if (tip.Name.Equals("Substation"))
                                {
                                    Substation sub = (Substation)it;
                                    float possibleIncrease = 0;
                                    int numberOfGen = 0;
                                    Dictionary<long, float> dicIncreaseValues = new Dictionary<long, float>();
                                    foreach (var item in sub.Equipments)
                                    {
                                        var generatorType = networkModel[item].GetType();
                                        if (generatorType.Name.Equals("Generator"))
                                        {
                                            if (!TurnedOffGenerators.Contains(item))
                                            {
                                                Generator generator = (Generator)networkModel[item];
                                                DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                {
                                                    if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                    {
                                                        possibleIncrease += generatorProduction.ActivePower * (generator.MaxFlexibility / 100);
                                                        dicIncreaseValues.Add(generator.GlobalId, generatorProduction.ActivePower * (generator.MaxFlexibility / 100));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    Dictionary<long, float> tempDicIncreaseValues = new Dictionary<long, float>();
                                    Dictionary<long, float> tempDicProcentValues = new Dictionary<long, float>();
                                    if (possibleIncrease >= hdpConsumption.ActivePower - hdpProduction.ActivePower)
                                    {
                                        foreach (var item in dicIncreaseValues)
                                        {
                                            float x = item.Value / (possibleIncrease / (hdpConsumption.ActivePower - hdpProduction.ActivePower));
                                            tempDicIncreaseValues.Add(item.Key, x);
                                        }
                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];
                                                    DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                    foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                    {
                                                        if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                        {
                                                            float percentValue = 0;
                                                            ///POgledaja ovo
                                                            percentValue = (tempDicIncreaseValues[generator.GlobalId] * 100) / generatorProduction.ActivePower;
                                                            tempDicProcentValues.Add(generator.GlobalId, percentValue);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];

                                                    if (generator.Flexibility)
                                                    {
                                                        DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                        foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                        {
                                                            if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                            {
                                                                if (generator.GeneratorType.Equals(GeneratorType.Solar))
                                                                {
                                                                    if (generatorProduction.ActivePower != 0)
                                                                        generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 + (tempDicProcentValues[generator.GlobalId] / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generator.GeneratorType.Equals(GeneratorType.Wind))
                                                                {
                                                                    if (generatorProduction.ActivePower != 0)
                                                                        generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 + (tempDicProcentValues[generator.GlobalId] / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }

                                                                if (generatorProduction.Time.Hour.Equals(DateTime.Now.Hour))
                                                                {
                                                                    dicForScada.Add(generator.GlobalId, tempDicProcentValues[generator.GlobalId]);
                                                                }


                                                            }
                                                        }
                                                        if (!changeFlexOfGen.Contains(generator.GlobalId))
                                                        {
                                                            changeFlexOfGen.Add(generator.GlobalId);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }

                                    else
                                    {
                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                if (!TurnedOffGenerators.Contains(item))
                                                {
                                                    Generator generator = (Generator)networkModel[item];

                                                    if (generator.Flexibility)
                                                    {
                                                        DerForecastDayAhead GeneratorderForecastDayAhead = prod[generator.GlobalId];
                                                        foreach (HourDataPoint generatorProduction in GeneratorderForecastDayAhead.Production.Hourly)
                                                        {
                                                            if (generatorProduction.Time.Equals(hdpProduction.Time))
                                                            {
                                                                if (generator.GeneratorType.Equals(GeneratorType.Solar))
                                                                {
                                                                    generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 + (generator.MaxFlexibility / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generator.GeneratorType.Equals(GeneratorType.Wind))
                                                                {
                                                                    generatorProduction.ActivePower = (float)(generatorProduction.ActivePower * (1 + (generator.MaxFlexibility / 100)));
                                                                    if (dicGener.ContainsKey(generator.GlobalId))
                                                                    {
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                    else
                                                                    {
                                                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                                                        dicGener[generator.GlobalId].Production.Hourly.Add(generatorProduction);
                                                                    }
                                                                }
                                                                if (generatorProduction.Time.Hour.Equals(DateTime.Now.Hour))
                                                                {
                                                                    dicForScada.Add(generator.GlobalId, generator.MaxFlexibility);
                                                                }

                                                            }
                                                        }
                                                        if (!changeFlexOfGen.Contains(generator.GlobalId))
                                                        {
                                                            changeFlexOfGen.Add(generator.GlobalId);
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }

                            }
                        }

                    }
                }

            }

            if (type.Name.Equals("Substation"))
            {

                Substation sub = (Substation)networkModel[GidUi];

                foreach (var item in sub.Equipments)
                {
                    var generatorType = networkModel[item].GetType();
                    if (generatorType.Name.Equals("Generator"))
                    {
                        if (!TurnedOffGenerators.Contains(item))
                        {
                            Generator generator = (Generator)networkModel[item];
                            Substation substation = (Substation)networkModel[generator.Container];
                            DerForecastDayAhead tempGen = new DerForecastDayAhead();
                            if (!dicGener.ContainsKey(generator.GlobalId))
                            {
                                tempGen.Production = prod[generator.GlobalId].Production;
                                tempDiffrence.Add(generator.GlobalId, tempGen);
                                dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                dicGener[generator.GlobalId].Production = prod[generator.GlobalId].Production.Clone();
                                dicGener[generator.GlobalId].Production -= dicGener[generator.GlobalId].Production;

                            }
                            else
                            {
                                tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                                tempDiffrence.Add(generator.GlobalId, tempGen);
                            }
                        }
                    }
                }
                DerForecastDayAhead tempSubValue = new DerForecastDayAhead();

                foreach (var item in dicGener)
                {
                    if (sub.Equipments.Contains(item.Key))
                    {
                        if (tempSubValue.Production.Hourly.Count == 0)
                        {
                            tempSubValue.Production = prod[item.Key].Production;
                        }
                        else
                        {
                            tempSubValue.Production += prod[item.Key].Production;
                        }
                    }
                }
                prod[sub.GlobalId].Production = tempSubValue.Production;
                
                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)networkModel[sub.SubGeoReg];
                GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subGeographicalRegion.GeoReg];

                DerForecastDayAhead tempSubRegValue = new DerForecastDayAhead();
                foreach (var item in subGeographicalRegion.Substations)
                {
                    if (tempSubRegValue.Production.Hourly.Count == 0)
                    {
                        tempSubRegValue.Production = prod[item].Production;
                    }
                    else
                    {
                        tempSubRegValue.Production += prod[item].Production;
                    }
                }
                prod[subGeographicalRegion.GlobalId].Production = tempSubRegValue.Production;
                DerForecastDayAhead tempGeoRegValue = new DerForecastDayAhead();
                foreach (var item in geographicalRegion.Regions)
                {
                    if (tempGeoRegValue.Production.Hourly.Count == 0)
                    {
                        tempGeoRegValue.Production = prod[item].Production;
                    }
                    else
                    {
                        tempGeoRegValue.Production += prod[item].Production;
                    }
                }
                prod[geographicalRegion.GlobalId].Production = tempGeoRegValue.Production;
                //prod[subGeographicalRegion.GlobalId].Production -= tempForecast.Production;
                //prod[geographicalRegion.GlobalId].Production -= tempForecast.Production;
            }

            if (type.Name.Equals("SubGeographicalRegion"))
            {
                SubGeographicalRegion subGeographicalRegionMain = (SubGeographicalRegion)networkModel[GidUi];

                foreach (var itemSub in subGeographicalRegionMain.Substations)
                {
                    Substation sub = (Substation)networkModel[itemSub];

                    foreach (var item in sub.Equipments)
                    {
                        var generatorType = networkModel[item].GetType();
                        if (generatorType.Name.Equals("Generator"))
                        {
                            if (!TurnedOffGenerators.Contains(item))
                            {
                                Generator generator = (Generator)networkModel[item];
                                Substation substation = (Substation)networkModel[generator.Container];
                                DerForecastDayAhead tempGen = new DerForecastDayAhead();
                                if (!dicGener.ContainsKey(generator.GlobalId))
                                {
                                    tempGen.Production = prod[generator.GlobalId].Production;
                                    tempDiffrence.Add(generator.GlobalId, tempGen);
                                    dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                    dicGener[generator.GlobalId].Production = prod[generator.GlobalId].Production.Clone();
                                    dicGener[generator.GlobalId].Production -= dicGener[generator.GlobalId].Production;

                                }
                                else
                                {
                                    tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                                    tempDiffrence.Add(generator.GlobalId, tempGen);
                                }
                            }
                        }
                    }
                    DerForecastDayAhead tempSubValue = new DerForecastDayAhead();

                    foreach (var item in dicGener)
                    {
                        if (sub.Equipments.Contains(item.Key))
                        {
                            if (tempSubValue.Production.Hourly.Count == 0)
                            {
                                tempSubValue.Production = prod[item.Key].Production;
                            }
                            else
                            {
                                tempSubValue.Production += prod[item.Key].Production;
                            }
                        }
                    }
                    prod[sub.GlobalId].Production = tempSubValue.Production;

                    SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)networkModel[sub.SubGeoReg];
                    GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subGeographicalRegion.GeoReg];

                    DerForecastDayAhead tempSubRegValue = new DerForecastDayAhead();
                    foreach (var item in subGeographicalRegion.Substations)
                    {
                        if (tempSubRegValue.Production.Hourly.Count == 0)
                        {
                            tempSubRegValue.Production = prod[item].Production;
                        }
                        else
                        {
                            tempSubRegValue.Production += prod[item].Production;
                        }
                    }
                    prod[subGeographicalRegion.GlobalId].Production = tempSubRegValue.Production;
                    DerForecastDayAhead tempGeoRegValue = new DerForecastDayAhead();
                    foreach (var item in geographicalRegion.Regions)
                    {
                        if (tempGeoRegValue.Production.Hourly.Count == 0)
                        {
                            tempGeoRegValue.Production = prod[item].Production;
                        }
                        else
                        {
                            tempGeoRegValue.Production += prod[item].Production;
                        }
                    }
                    prod[geographicalRegion.GlobalId].Production = tempGeoRegValue.Production;

                }
            }

            if (type.Name.Equals("GeographicalRegion"))
            {
                GeographicalRegion geographicalRegionMain = (GeographicalRegion)networkModel[GidUi];

                foreach (var itemGeo in geographicalRegionMain.Regions)
                {


                    SubGeographicalRegion subGeographicalRegionMain = (SubGeographicalRegion)networkModel[itemGeo];

                    foreach (var itemSub in subGeographicalRegionMain.Substations)
                    {
                        Substation sub = (Substation)networkModel[itemSub];

                        foreach (var item in sub.Equipments)
                        {
                            var generatorType = networkModel[item].GetType();
                            if (generatorType.Name.Equals("Generator"))
                            {
                                if (!TurnedOffGenerators.Contains(item))
                                {
                                    Generator generator = (Generator)networkModel[item];
                                    Substation substation = (Substation)networkModel[generator.Container];
                                    DerForecastDayAhead tempGen = new DerForecastDayAhead();
                                    if (!dicGener.ContainsKey(generator.GlobalId))
                                    {
                                        tempGen.Production = prod[generator.GlobalId].Production;
                                        tempDiffrence.Add(generator.GlobalId, tempGen);
                                        dicGener.Add(generator.GlobalId, new DerForecastDayAhead());
                                        dicGener[generator.GlobalId].Production = prod[generator.GlobalId].Production.Clone();
                                        dicGener[generator.GlobalId].Production -= dicGener[generator.GlobalId].Production;

                                    }
                                    else
                                    {
                                        tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                                        tempDiffrence.Add(generator.GlobalId, tempGen);
                                    }
                                }
                            }
                        }
                        DerForecastDayAhead tempSubValue = new DerForecastDayAhead();

                        foreach (var item in dicGener)
                        {
                            if (sub.Equipments.Contains(item.Key))
                            {
                                if (tempSubValue.Production.Hourly.Count == 0)
                                {
                                    tempSubValue.Production = prod[item.Key].Production;
                                }
                                else
                                {
                                    tempSubValue.Production += prod[item.Key].Production;
                                }
                            }
                        }
                        prod[sub.GlobalId].Production = tempSubValue.Production;

                        SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)networkModel[sub.SubGeoReg];
                        GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subGeographicalRegion.GeoReg];

                        DerForecastDayAhead tempSubRegValue = new DerForecastDayAhead();
                        foreach (var item in subGeographicalRegion.Substations)
                        {
                            if (tempSubRegValue.Production.Hourly.Count == 0)
                            {
                                tempSubRegValue.Production = prod[item].Production;
                            }
                            else
                            {
                                tempSubRegValue.Production += prod[item].Production;
                            }
                        }
                        prod[subGeographicalRegion.GlobalId].Production = tempSubRegValue.Production;
                        DerForecastDayAhead tempGeoRegValue = new DerForecastDayAhead();
                        foreach (var item in geographicalRegion.Regions)
                        {
                            if (tempGeoRegValue.Production.Hourly.Count == 0)
                            {
                                tempGeoRegValue.Production = prod[item].Production;
                            }
                            else
                            {
                                tempGeoRegValue.Production += prod[item].Production;
                            }
                        }
                        prod[geographicalRegion.GlobalId].Production = tempGeoRegValue.Production;

                    }
                }
            }
            foreach (long gidOfGen in changeFlexOfGen)
            {
                Generator generator = (Generator)networkModel[gidOfGen];

                generator.Flexibility = false;

            }
            //---NIJE RESENO---DODATNA KONEKCIJA
            //CalculationEngineCache.Instance.ListOfGenerators = dicForScada;
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );
            foreach (KeyValuePair<long, double> kvp in dicForScada)
            {
                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToListOfGeneratorsForScada(kvp.Key, kvp.Value)).Wait();
            }
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.UpdateProductionCached(prod)).Wait();
            /*
             new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<ISendListOfGeneratorsToScada>(
                        wcfServiceObject: new CommandingService(),
                        serviceContext: context,
                        endpointResourceName: "SCADACommandingMicroserviceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "SCADACommandingMicroserviceListener"
                )
             */

            //CloudClient<IScadaCloudToScadaLocal> transactionCoordinatorScadaaaaa = new CloudClient<IScadaCloudToScadaLocal>
            //(
            //  serviceUri: new Uri("fabric:/CalculateEngineApp/SCADACacheMicroservice"),
            //  partitionKey:  ServicePartitionKey.Singleton,
            //  clientBinding: WcfUtility.CreateTcpClientBinding(),
            //  listenerName: "SCADAComunicationMicroserviceListener"
            //);

            //string sss = transactionCoordinatorScadaaaaa.InvokeWithRetryAsync(client => client.Channel.GetAddress()).Result;

            CloudClient<ISendListOfGeneratorsToScada> transactionCoordinatorScada = new CloudClient<ISendListOfGeneratorsToScada>
            (
              serviceUri: new Uri("fabric:/SCADAApp/SCADACommandMicroservice"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "SCADACommandingMicroserviceListener"
            );


            transactionCoordinatorScada.InvokeWithRetryAsync(client => client.Channel.SendListOfGenerators(dicForScada)).Wait();


            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.UpdateMinAndMaxFlexibilityForChangedGenerators(dicForScada)).Wait();

            return energyFromSource;
        }

        public async Task<float> BalanceNetworkModel()
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            float energyFromSource = 0;
            foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
            {
                var type = kvp.Value.GetType();
                if (type.Name.Equals("GeographicalRegion"))
                {
                    energyFromSource += UpdateThroughUI(kvp.Key).Result;
                }
            }
            ///POGLEDAJ METODA OD KESA
            await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead());
            //CalculationEngineCache.Instance.NetworkModelBalanced();
            return energyFromSource;
        }

        public async Task<List<long>> AllGeoRegions()
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            List<long> geoReg = new List<long>();
            foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
            {
                var type = kvp.Value.GetType();
                if (type.Name.Equals("GeographicalRegion"))
                {
                    geoReg.Add(kvp.Key);
                }
            }
            return geoReg;

        }

        //METODA IZMENJENA ZA CLOUD ->>>TESTIRATI DA LI RADI
        public async Task<List<long>> AllowOptimization(long gid)
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            List<long> DisableAutomaticOptimization = new List<long>();
            Dictionary<int, List<long>> tempDisableAutomaticOptimization = new Dictionary<int, List<long>>();
            int GeographicalRegionsCount = 0;
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            tempDisableAutomaticOptimization = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetDisableAutomaticOptimization()).Result;
            DisableAutomaticOptimization = tempDisableAutomaticOptimization[0];
            foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
            {
                IdentifiedObject io = kvp.Value;
                var type = io.GetType();

                if (type.Name.Equals("GeographicalRegion"))
                {
                    GeographicalRegionsCount++;
                }
            }
            if (gid == -1)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
                {
                    IdentifiedObject io = kvp.Value;
                    var type = io.GetType();

                    if (type.Name.Equals("GeographicalRegion") || type.Name.Equals("SubGeographicalRegion") || type.Name.Equals("Substation"))
                    {
                        if (!DisableAutomaticOptimization.Contains(kvp.Key))
                        {
                            DisableAutomaticOptimization.Add(kvp.Key);
                        }
                    }
                }
            }
            if (networkModel.ContainsKey(gid))
            {
                IdentifiedObject io = networkModel[gid];
                var type = io.GetType();

                if (type.Name.Equals("GeographicalRegion"))
                {
                    GeographicalRegion geographicalRegion = (GeographicalRegion)io;
                    if (!DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                    {
                        DisableAutomaticOptimization.Add(geographicalRegion.GlobalId);
                    }
                    foreach (var item in geographicalRegion.Regions)
                    {
                        SubGeographicalRegion subgeo = (SubGeographicalRegion)networkModel[item];
                        if (!DisableAutomaticOptimization.Contains(subgeo.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(subgeo.GlobalId);
                        }
                        foreach (var sub in subgeo.Substations)
                        {
                            Substation substation = (Substation)networkModel[sub];
                            if (!DisableAutomaticOptimization.Contains(substation.GlobalId))
                            {
                                DisableAutomaticOptimization.Add(substation.GlobalId);
                            }
                        }
                    }
                    if (GeographicalRegionsCount == 1)
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }
                    bool tempProvera = true;
                    foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
                    {
                        IdentifiedObject io2 = kvp.Value;
                        var type2 = io.GetType();

                        if (type2.Name.Equals("GeographicalRegion"))
                        {
                            if (!DisableAutomaticOptimization.Contains(kvp.Key))
                            {
                                tempProvera = false;
                                break;
                            }
                        }
                    }
                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }

                }
                if (type.Name.Equals("SubGeographicalRegion"))
                {
                    SubGeographicalRegion subgeo = (SubGeographicalRegion)io;
                    if (!DisableAutomaticOptimization.Contains(subgeo.GlobalId))
                    {
                        DisableAutomaticOptimization.Add(subgeo.GlobalId);
                    }
                    foreach (var sub in subgeo.Substations)
                    {
                        Substation substation = (Substation)networkModel[sub];
                        if (!DisableAutomaticOptimization.Contains(substation.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(substation.GlobalId);
                        }
                    }
                    GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subgeo.GeoReg];
                    if (geographicalRegion.Regions.Count == 1)
                    {
                        if (!DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(geographicalRegion.GlobalId);
                        }
                    }
                    bool tempProvera = true;
                    foreach (var item in geographicalRegion.Regions)
                    {
                        if (!DisableAutomaticOptimization.Contains(item))
                        {
                            tempProvera = false;
                            break;
                        }
                    }
                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(geographicalRegion.GlobalId);
                        }
                    }
                    if (GeographicalRegionsCount == 1 && DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }
                    tempProvera = true;
                    foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
                    {
                        IdentifiedObject io2 = kvp.Value;
                        var type2 = io.GetType();

                        if (type2.Name.Equals("GeographicalRegion"))
                        {
                            if (!DisableAutomaticOptimization.Contains(kvp.Key))
                            {
                                tempProvera = false;
                                break;
                            }
                        }
                    }

                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }
                }
                if (type.Name.Equals("Substation"))
                {
                    Substation substation = (Substation)io;
                    if (!DisableAutomaticOptimization.Contains(substation.GlobalId))
                    {
                        DisableAutomaticOptimization.Add(substation.GlobalId);
                    }
                    SubGeographicalRegion subgeo = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                    if (subgeo.Substations.Count == 1)
                    {
                        if (!DisableAutomaticOptimization.Contains(subgeo.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(subgeo.GlobalId);
                        }
                    }
                    bool tempProvera = true;
                    foreach (var item in subgeo.Substations)
                    {
                        if (!DisableAutomaticOptimization.Contains(item))
                        {
                            tempProvera = false;
                            break;
                        }
                    }
                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(subgeo.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(subgeo.GlobalId);
                        }
                    }
                    tempProvera = true;
                    GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subgeo.GeoReg];
                    if (geographicalRegion.Regions.Count == 1 && DisableAutomaticOptimization.Contains(subgeo.GlobalId))
                    {
                        if (!DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(geographicalRegion.GlobalId);
                        }
                    }
                    foreach (var item in geographicalRegion.Regions)
                    {
                        if (!DisableAutomaticOptimization.Contains(item))
                        {
                            tempProvera = false;
                            break;
                        }
                    }
                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                        {
                            DisableAutomaticOptimization.Add(geographicalRegion.GlobalId);
                        }
                    }
                    if (GeographicalRegionsCount == 1 && DisableAutomaticOptimization.Contains(geographicalRegion.GlobalId))
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }
                    tempProvera = true;
                    foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
                    {
                        IdentifiedObject io2 = kvp.Value;
                        var type2 = io.GetType();

                        if (type2.Name.Equals("GeographicalRegion"))
                        {
                            if (!DisableAutomaticOptimization.Contains(kvp.Key))
                            {
                                tempProvera = false;
                                break;
                            }
                        }
                    }
                    if (tempProvera)
                    {
                        if (!DisableAutomaticOptimization.Contains(-1))
                        {
                            DisableAutomaticOptimization.Add(-1);
                        }
                    }
                }
            }

            return DisableAutomaticOptimization;
        }
        public async Task<List<long>> ListOfDisabledGenerators()
        {
            List<long> DisableAutomaticOptimization = new List<long>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );
            Dictionary<int, List<long>> tempDisableAutomaticOptimization = new Dictionary<int, List<long>>();
            tempDisableAutomaticOptimization = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetDisableAutomaticOptimization()).Result;
            if(tempDisableAutomaticOptimization.Count>0)
                DisableAutomaticOptimization = tempDisableAutomaticOptimization[0];
            return DisableAutomaticOptimization;
        }
        public async Task<List<Generator>> ListOffTurnedOffGenerators()
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            List<long> TurnedOnGenerators = new List<long>();
            List<long> DisableAutomaticOptimization = new List<long>();
            List<Generator> generators = new List<Generator>();

            Dictionary<int, List<long>> tempTurnedOnGenerators = new Dictionary<int, List<long>>();
            Dictionary<int, List<long>> tempDisableAutomaticOptimization = new Dictionary<int, List<long>>();


            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            tempTurnedOnGenerators = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTurnedOnGenerators()).Result;
            if(tempTurnedOnGenerators.Count>0)
                TurnedOnGenerators = tempTurnedOnGenerators[0];
            tempDisableAutomaticOptimization = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetDisableAutomaticOptimization()).Result;
            if (tempDisableAutomaticOptimization.Count > 0)
                DisableAutomaticOptimization = tempDisableAutomaticOptimization[0];

            foreach (long gid in TurnedOnGenerators)
            {
                Generator generator = (Generator)networkModel[gid];
                if (generator.Flexibility)
                {
                    if (DisableAutomaticOptimization.Contains(generator.Container))
                    {
                        Substation substation = (Substation)networkModel[generator.Container];
                        DisableAutomaticOptimization.Remove(substation.GlobalId);
                        if (DisableAutomaticOptimization.Contains(substation.SubGeoReg))
                        {
                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                            DisableAutomaticOptimization.Remove(subGeographicalRegion.GlobalId);
                            if (DisableAutomaticOptimization.Contains(subGeographicalRegion.GeoReg))
                            {
                                GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subGeographicalRegion.GeoReg];
                                DisableAutomaticOptimization.Remove(geographicalRegion.GlobalId);
                            }
                        }
                    }
                }
                generators.Add(generator);
            }

            return generators;
        }
        public async Task<List<Generator>> GeneratorOffCheck()
        {//Ima gresaka 

            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            List<long> TurnedOffGenerators = new List<long>();

            Dictionary<int, List<long>> tempTurnedOffGenerators = new Dictionary<int, List<long>>();

            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );
            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            tempTurnedOffGenerators = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTurnedOffGenerators()).Result;
            if(tempTurnedOffGenerators.Count>0)
                TurnedOffGenerators = tempTurnedOffGenerators[0]; /// 

            List<Generator> generators = new List<Generator>();
            foreach (long genGid in TurnedOffGenerators)
            {
                Generator generator = (Generator)networkModel[genGid];
                generators.Add(generator);
            }
            return generators;
        }
    }
}