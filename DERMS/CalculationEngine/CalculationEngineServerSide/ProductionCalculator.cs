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
    public class ProductionCalculator
    {
        private Dictionary<long, Forecast> subGeoRegionWeather;
        private NetworkModelTransfer networkModel;

        public ProductionCalculator(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;

            //InitializeWeather();
        }

        //private void InitializeWeather()
        //{
        //    subGeoRegionWeather = new Dictionary<long, Forecast>();
        //    foreach (SubGeographicalRegion subGeoRegion in networkModel.SubGeoRegions)
        //    {
        //        Forecast forecast = new Forecast();
        //        forecast.Hourly = new HourlyForecast();
        //        forecast.Hourly.Hours = new List<DarkSkyApi.Models.HourDataPoint>();
        //        for (int i = 0; i < 25; i++)
        //        {
        //            forecast.Hourly.Hours.Add(new DarkSkyApi.Models.HourDataPoint());
        //        }
        //        subGeoRegionWeather.Add(subGeoRegion.Gid, forecast);
        //    }
        //}

        public Dictionary<long, DerForecastDayAhead> CalculateSubstations(Forecast forecast)
        {
            Dictionary<long, DerForecastDayAhead> substationsForecast = new Dictionary<long, DerForecastDayAhead>();
            List<Generator> generators = new List<Generator>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var generator = (Generator)kvpDic.Value;
                        generators.Add(generator);
                    }
                }
            }
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var substation = (Substation)kvpDic.Value;
                        DerForecastDayAhead substationForecast = new DerForecastDayAhead(substation.GlobalId);
                        foreach (Generator generator in generators)
                        {
                            if (substation.Equipments.Contains(generator.GlobalId))
                            {
                                DayAhead dayAhead = generator.CalculateDayAhead(forecast, substation.GlobalId, substation);
                                substationForecast.Production += dayAhead;

                                substationsForecast[substation.GlobalId] = substationForecast;
                            }
                        }

                    }

                }
            }

            return substationsForecast;
        }

        public DerForecastDayAhead CalculateSubstation(Forecast forecast, Substation substation)
        {
            Dictionary<long, DerForecastDayAhead> substationsForecast = new Dictionary<long, DerForecastDayAhead>();
            List<Generator> generators = new List<Generator>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var generator = (Generator)kvpDic.Value;
                        generators.Add(generator);
                    }
                }
            }


            DerForecastDayAhead substationForecast = new DerForecastDayAhead(substation.GlobalId);
            foreach (Generator generator in generators)
            {
                if (substation.Equipments.Contains(generator.GlobalId))
                {
                    DayAhead dayAhead = generator.CalculateDayAhead(forecast, substation.GlobalId, substation);
                    substationForecast.Production += dayAhead;

                    substationsForecast[substation.GlobalId] = substationForecast;
                }
            }



            return substationForecast;
        }
    }
}
