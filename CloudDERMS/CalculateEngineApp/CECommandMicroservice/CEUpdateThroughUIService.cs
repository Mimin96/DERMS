using CalculationEngineServiceCommon;
using CloudCommon;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using CloudCommon.SCADA;
using DERMSCommon;
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
    public class CEUpdateThroughUIService : MessageWriting, ICEUpdateThroughUI
    {
        public async Task UpdateThroughUI(long data)
        {
            MessageReceivedEvent("Information: Automatic Optimization started.");
            PopulateBalance(data).Wait();

            CloudClient<IEvetnsDatabase> transactionCoordinator = new CloudClient<IEvetnsDatabase>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0), /*CJN*/
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "SetEventsToDatabaseListener"
            );
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SetEvent(new Event("Automatic optimization is executed. ", Enums.Component.CalculationEngine, DateTime.Now))).Wait();
        }

        public async Task PopulateBalance(long gid)
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

            Balance(productionCachedDictionary, gid, nmsCacheDictionary, turnedOffGeneratorsList).Wait();
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead()).Wait();
        }

        public async Task Balance(Dictionary<long, DerForecastDayAhead> prod, long GidUi, Dictionary<long, IdentifiedObject> networkModel, List<long> TurnedOffGenerators)
        {

            Dictionary<long, double> battery = new Dictionary<long, double>();
            Dictionary<long, List<long>> energySources = new Dictionary<long, List<long>>();

            Dictionary<long, DerForecastDayAhead> dicGener = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DerForecastDayAhead> tempDiffrence = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, double> dicForScada = new Dictionary<long, double>();
            List<long> changeFlexOfGen = new List<long>();

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

            try
            {
                transactionCoordinatorScada.InvokeWithRetryAsync(client => client.Channel.SendListOfGenerators(dicForScada)).Wait();
            }
            catch (AggregateException ex)
            {

            }

            try
            {
                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.UpdateMinAndMaxFlexibilityForChangedGenerators(dicForScada)).Wait();
            }
            catch (AggregateException ex2)
            {

            }
        }

        public async Task BalanceNetworkModel()
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
                    UpdateThroughUI(kvp.Key);
                }
            }
            ///POGLEDAJ METODA OD KESA
            await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead());
            //CalculationEngineCache.Instance.NetworkModelBalanced();
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
            List<NetworkModelTreeClass> NetworkModelTreeClass = new List<NetworkModelTreeClass>();
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
            NetworkModelTreeClass = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNetworkModelTreeClass()).Result;
            if(tempDisableAutomaticOptimization.Count != 0)
                DisableAutomaticOptimization = tempDisableAutomaticOptimization[0];
            foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModelTreeClass)
            {
                if (gid.Equals(networkModelTreeClasses.GID))
                {
                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                    {
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                    }
                    foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                    {
                        if (!DisableAutomaticOptimization.Contains(gr.GID))
                        {
                            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                            DisableAutomaticOptimization.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sgr.GID)).Wait();
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sub.GID)).Wait();
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }

                        }
                    }
                }


                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {
                    if (gid.Equals(gr.GID))
                    {
                        if (!DisableAutomaticOptimization.Contains(gr.GID))
                        {
                            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                            DisableAutomaticOptimization.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sgr.GID)).Wait();
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sub.GID)).Wait();
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }

                        }
                        if (networkModelTreeClasses.GeographicalRegions.Count == 1)
                        {
                            if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                            {
                                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                            }
                        }
                        bool tempProvera = true;
                        foreach (var item in networkModelTreeClasses.GeographicalRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(item.GID))
                            {
                                tempProvera = false;
                                break;
                            }
                        }
                        if (tempProvera)
                        {
                            if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                            {
                                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                            }
                        }
                    }

                }

                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {
                        if (gid.Equals(sgr.GID))
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sgr.GID)).Wait();
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }

                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sub.GID)).Wait();
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }

                            if (gr.GeographicalSubRegions.Count == 1)
                            {
                                if (!DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                                    DisableAutomaticOptimization.Add(gr.GID);
                                }
                            }
                            bool tempProvera = true;
                            foreach (var item in gr.GeographicalSubRegions)
                            {
                                if (!DisableAutomaticOptimization.Contains(item.GID))
                                {
                                    tempProvera = false;
                                    break;
                                }
                            }
                            if (tempProvera)
                            {
                                if (!DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                                    DisableAutomaticOptimization.Add(gr.GID);
                                }
                            }
                            if (networkModelTreeClasses.GeographicalRegions.Count == 1 && DisableAutomaticOptimization.Contains(gr.GID))
                            {
                                if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                    DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                }
                            }
                            tempProvera = true;
                            foreach (var item in networkModelTreeClasses.GeographicalRegions)
                            {
                                if (!DisableAutomaticOptimization.Contains(item.GID))
                                {
                                    tempProvera = false;
                                    break;
                                }
                            }
                            if (tempProvera)
                            {
                                if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                    DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                }
                            }

                        }

                    }


                }
                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {

                        foreach (SubstationTreeClass sub in sgr.Substations)
                        {
                            if (gid.Equals(sub.GID))
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sub.GID)).Wait();
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }
                                if (sgr.Substations.Count == 1)
                                {
                                    if (!DisableAutomaticOptimization.Contains(sgr.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sgr.GID)).Wait();
                                        DisableAutomaticOptimization.Add(sgr.GID);
                                    }
                                }
                                bool tempProvera = true;
                                foreach (var item in sgr.Substations)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(sgr.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(sgr.GID)).Wait();
                                        DisableAutomaticOptimization.Add(sgr.GID);
                                    }
                                }
                                tempProvera = true;
                                if (gr.GeographicalSubRegions.Count == 1 && DisableAutomaticOptimization.Contains(sgr.GID))
                                {
                                    if (!DisableAutomaticOptimization.Contains(gr.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                                        DisableAutomaticOptimization.Add(gr.GID);
                                    }
                                }
                                foreach (var item in gr.GeographicalSubRegions)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(gr.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(gr.GID)).Wait();
                                        DisableAutomaticOptimization.Add(gr.GID);
                                    }
                                }
                                if (networkModelTreeClasses.GeographicalRegions.Count == 1 && DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                    }
                                }
                                tempProvera = true;
                                foreach (var item in networkModelTreeClasses.GeographicalRegions)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                    {
                                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToDisableAutomaticOptimization(networkModelTreeClasses.GID)).Wait();
                                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                    }
                                }


                            }

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
                        await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTurnedOnGenerators(generator.GlobalId));
                        await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromDisableAutomaticOptimization(substation.GlobalId)); 
                        if (DisableAutomaticOptimization.Contains(substation.SubGeoReg))
                        {
                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                            DisableAutomaticOptimization.Remove(subGeographicalRegion.GlobalId);
                            await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTurnedOnGenerators(subGeographicalRegion.GlobalId));
                            await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromDisableAutomaticOptimization(subGeographicalRegion.GlobalId));
                            if (DisableAutomaticOptimization.Contains(subGeographicalRegion.GeoReg))
                            {
                                GeographicalRegion geographicalRegion = (GeographicalRegion)networkModel[subGeographicalRegion.GeoReg];
                                DisableAutomaticOptimization.Remove(geographicalRegion.GlobalId);
                                await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTurnedOnGenerators(geographicalRegion.GlobalId));
                                await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromDisableAutomaticOptimization(geographicalRegion.GlobalId));
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