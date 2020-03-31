using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkSkyApi;
using DarkSkyApi.Models;
using DERMSCommon;

namespace WeatherForecast
{
    public class DarkSkyApi
    {
        private DarkSkyService darkSkyProxy;

        public DarkSkyApi()
        {
            // fa6d00664c0c9abf42654341ff91db31
            // e67254e31e12e23461c61e0fb0489142
            darkSkyProxy = new DarkSkyService("e67254e31e12e23461c61e0fb0489142");
        }

        public async Task<Forecast> GetWeatherForecastAsync(double latitude, double longitude)
        {
            Forecast result = await darkSkyProxy.GetTimeMachineWeatherAsync(longitude, latitude, DateTime.Now, Unit.Auto);
            List<HourDataPoint> hourDataPoints =  result.Hourly.Hours.ToList();

            DERMSCommon.WeatherForecast.WeatherForecast weatherForecast = new DERMSCommon.WeatherForecast.WeatherForecast(1001,1, 1, 1, 1, DateTime.Now, "");

            return result;
        }
    }
}
