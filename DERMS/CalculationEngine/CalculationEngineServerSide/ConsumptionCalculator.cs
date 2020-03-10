using DarkSkyApi.Models;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class ConsumptionCalculator
    {
        private NetworkModelTransfer networkModel;
        private Dictionary<long, DerForecastDayAhead> Forecasts;
        private Dictionary<long, DayAhead> substationDayAhead;
        private ConsumerCharacteristics consumerCharacteristics = new ConsumerCharacteristics();
        private Dictionary<DateTime, double> consumptionPerHour = new Dictionary<DateTime, double>();

        public ConsumptionCalculator(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;
        }

        public void Calculate(Dictionary<long,DerForecastDayAhead> derForcast)
        {
            this.Forecasts = derForcast;
            CalculateDayAheadSubstation();
            CalculateSubstations(derForcast);
            CalculateSubRegion(derForcast);
            CalculateGeoRegions(derForcast);
        }

        private void CalculateDayAheadSubstation()
        {
            List<EnergyConsumer> energyConsumers = new List<EnergyConsumer>();
            energyConsumers = GetEnergyConsumers();
            substationDayAhead = new Dictionary<long, DayAhead>();

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    DayAhead consumerDayAhead = consumerCharacteristics.GetDayAhead();

                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        
                        Forecast forecast = CalculationEngineCache.Instance.GetForecast(kvpDic.Key);

                        foreach(DERMSCommon.WeatherForecast.HourDataPoint dataPoint in consumerDayAhead.Hourly)
                        {
                            DarkSkyApi.Models.HourDataPoint hourDataPoint = forecast.Hourly.Hours.FirstOrDefault(h => h.Time.Hour == dataPoint.Time.Hour);
                            float curveFactor = 0;
                            curveFactor = dataPoint.ActivePower;
                            dataPoint.ActivePower = 0;
                            foreach(EnergyConsumer ec in energyConsumers)
                            {
                                if(gr.Equipments.Contains(ec.GlobalId))
                                {
                                    dataPoint.ActivePower += ec.PFixed * curveFactor;
                                }
                            }
                            
                        }
                        substationDayAhead.Add(kvpDic.Key, consumerDayAhead.Clone());
                    }
                    
                }
                
            }
        }

        public void CalculateSubstations(Dictionary<long, DerForecastDayAhead> derForcast)
        {
            Dictionary<long, DerForecastDayAhead> substationForecast = Forecasts;
            foreach(KeyValuePair<long,DerForecastDayAhead> kvp in derForcast)
            {
                foreach (KeyValuePair<long, DayAhead> kvp2 in substationDayAhead)
                {
                    if (kvp.Key.Equals(kvp2.Key))
                    {
                        kvp.Value.Consumption += substationDayAhead[kvp.Key];
                    }
                }
            }
        }


        public void CalculateSubRegion(Dictionary<long, DerForecastDayAhead> derForcast)
        {
            List<Substation> substations = new List<Substation>();
            substations = GetSubstations();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        derForcast[gr.GlobalId].Consumption = new DayAhead();
                        foreach(Substation substation in substations)
                        {
                            if(gr.Substations.Contains(substation.GlobalId))
                            {
                                derForcast[gr.GlobalId].Consumption += derForcast[substation.GlobalId].Consumption;
                            }
                        }
                    }
                }
            }
        }

        public void CalculateGeoRegions(Dictionary<long, DerForecastDayAhead> derForcast)
        {
            List<SubGeographicalRegion> geographicalRegions = new List<SubGeographicalRegion>();
            geographicalRegions = GetSubGeographicalRegions();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        derForcast[gr.GlobalId].Consumption = new DayAhead();
                        foreach (SubGeographicalRegion subGeoRegion in geographicalRegions)
                        {
                            if (gr.Regions.Contains(subGeoRegion.GlobalId))
                            {
                                derForcast[gr.GlobalId].Consumption += derForcast[subGeoRegion.GlobalId].Consumption;
                            }
                        }
                    }
                }
            }
        }

        public List<EnergyConsumer> GetEnergyConsumers()
        {
            List<EnergyConsumer> energyConsumers = new List<EnergyConsumer>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }

        public List<Substation> GetSubstations()
        {
            List<Substation> energyConsumers = new List<Substation>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }

        public List<SubGeographicalRegion> GetSubGeographicalRegions()
        {
            List<SubGeographicalRegion> energyConsumers = new List<SubGeographicalRegion>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }

    }
}
