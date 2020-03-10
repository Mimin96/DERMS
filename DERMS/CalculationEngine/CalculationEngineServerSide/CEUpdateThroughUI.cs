using CalculationEngineServiceCommon;
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

                                if (energySources.ContainsKey(kvp.Key))
                                {
                                    int distributionFactor = energySources[kvp.Key].Count;
                                    foreach (long es in energySources[kvp.Key])
                                    {
                                        EnergySource energySource = (EnergySource)networkModel[es];
                                        energySource.ActivePower -= (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor;
                                    }

                                    hdpProduction.ActivePower = hdpConsumption.ActivePower;
                                }
                            }
                            else if (hdpProduction.ActivePower < hdpConsumption.ActivePower)
                            {

                                if (energySources.ContainsKey(kvp.Key))
                                {
                                    int distributionFactor = energySources[kvp.Key].Count;
                                    foreach (long es in energySources[kvp.Key])
                                    {
                                        EnergySource energySource = (EnergySource)networkModel[es];
                                        if (energySource.ActivePower >= (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor)
                                        {
                                            energySource.ActivePower -= (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor;
                                            energyFromSource += (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor;
                                        }
                                    }
                                    hdpProduction.ActivePower = hdpConsumption.ActivePower;
                                }
                                else
                                {
                                    //Turn off consumers...
                                }


                                //}
                                //else
                                //{
                                //    if (energySources.ContainsKey(kvp.Key))
                                //    {
                                //        int distributionFactor = energySources[kvp.Key].Count;
                                //        foreach (long es in energySources[kvp.Key])
                                //        {
                                //            EnergySource energySource = (EnergySource)networkModel[es];
                                //            if (energySource.ActivePower >= (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor)
                                //            {
                                //                energySource.ActivePower -= (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor;
                                //                energyFromSource += (hdpConsumption.ActivePower - hdpProduction.ActivePower) / distributionFactor;
                                //            }
                                //        }
                                //        hdpProduction.ActivePower = hdpConsumption.ActivePower;
                                //    }
                                //    else
                                //    {
                                //        //Turn off consumers...
                                //    }
                                //}
                            }
                        }
                    }
                }
            }

            return energyFromSource;
        }
    }
}