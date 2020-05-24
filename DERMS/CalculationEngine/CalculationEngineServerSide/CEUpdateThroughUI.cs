﻿using CalculationEngineServiceCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class CEUpdateThroughUI : ICEUpdateThroughUI
    {
        private Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
        private Dictionary<long, double> batteryStorage = new Dictionary<long, double>();
   
        public float UpdateThroughUI(long data)
        {
            float energyFromSource = CalculationEngineCache.Instance.PopulateBalance(data);
            return energyFromSource;
        }
        public float Balance(Dictionary<long, DerForecastDayAhead> prod, long GidUi)
        {
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
            Dictionary<long, double> battery = new Dictionary<long, double>();
            Dictionary<long, List<long>> energySources = new Dictionary<long, List<long>>();

            Dictionary<long, DerForecastDayAhead> dicGener = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DerForecastDayAhead> tempDiffrence = new Dictionary<long, DerForecastDayAhead>();

            IdentifiedObject io = networkModel[GidUi];
            var type = io.GetType();
            float energyFromSource = 0;
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

                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)networkModel[item];
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
                                                Generator generator = (Generator)networkModel[item];
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

                                        foreach (var item in sub.Equipments)
                                        {
                                            var generatorType = networkModel[item].GetType();
                                            if (generatorType.Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)networkModel[item];
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

                                                Generator generator = (Generator)networkModel[item];
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
                        Generator generator = (Generator)networkModel[item];
                        Substation substation = (Substation)networkModel[generator.Container];
                        DerForecastDayAhead tempGen = new DerForecastDayAhead();
                        tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                        tempDiffrence.Add(generator.GlobalId, tempGen);
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
                            Generator generator = (Generator)networkModel[item];
                            Substation substation = (Substation)networkModel[generator.Container];
                            DerForecastDayAhead tempGen = new DerForecastDayAhead();
                            tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                            tempDiffrence.Add(generator.GlobalId, tempGen);
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
                                Generator generator = (Generator)networkModel[item];
                                Substation substation = (Substation)networkModel[generator.Container];
                                DerForecastDayAhead tempGen = new DerForecastDayAhead();
                                tempGen.Production = prod[generator.GlobalId].Production - dicGener[generator.GlobalId].Production;
                                tempDiffrence.Add(generator.GlobalId, tempGen);
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
                }
            }



            return energyFromSource;
        }

        public float BalanceNetworkModel()
        {
            float energyFromSource = 0;
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
            foreach (KeyValuePair<long, IdentifiedObject> kvp in networkModel)
            {
                var type = kvp.Value.GetType();
                if (type.Name.Equals("GeographicalRegion"))
                {
                    energyFromSource += UpdateThroughUI(kvp.Key);
                }
            }
            CalculationEngineCache.Instance.NetworkModelBalanced();
            return energyFromSource;
        }

        public List<long> AllGeoRegions()
        {
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
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
    }
}