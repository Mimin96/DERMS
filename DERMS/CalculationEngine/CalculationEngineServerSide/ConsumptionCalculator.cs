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

        public ConsumptionCalculator(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;
        }

        public void Calculate(Dictionary<long,DerForecastDayAhead> derForcast)
        {
            this.Forecasts = derForcast;
            CalculateDayAheadSubstation();
            CalculateSubstations();
        }

        private void CalculateDayAheadSubstation()
        {
            substationDayAhead = new Dictionary<long, DayAhead>();
            DayAhead consumerDayAhead = consumerCharacteristics.GetDayAhead();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        
                        Forecast forecast = CalculationEngineCache.Instance.GetForecast(kvpDic.Key);

                        foreach(DERMSCommon.WeatherForecast.HourDataPoint dataPoint in consumerDayAhead.Hourly)
                        {
                            DarkSkyApi.Models.HourDataPoint hourDataPoint = forecast.Hourly.Hours.FirstOrDefault(h => h.Time.Hour == dataPoint.Time.Hour);

                            float activePowerAddition = 0;
                            if(hourDataPoint.Temperature < 10)
                            {
                                activePowerAddition += 15 + (5 - hourDataPoint.Temperature) * 2;
                            }
                            else if(hourDataPoint.Temperature > 25)
                            {
                                activePowerAddition += 20 + hourDataPoint.Temperature * 3;
                            }
                            dataPoint.ActivePower += activePowerAddition;
                        }
                    }
                    substationDayAhead.Add(kvpDic.Key, consumerDayAhead);
                }
                
            }
        }

        public void CalculateSubstations()
        {
            Dictionary<long, DerForecastDayAhead> substationForecast = Forecasts;
            foreach(KeyValuePair<long,DerForecastDayAhead> kvp in substationForecast)
            {
                kvp.Value.Consumption += substationForecast[kvp.Key].Consumption;
            }
        }

    }
}
