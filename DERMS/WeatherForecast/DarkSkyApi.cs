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
            darkSkyProxy = new DarkSkyService("e67254e31e12e23461c61e0fb0489142");
        }

        public async Task<DERMSCommon.WeatherForecast.WeatherForecast> GetWeatherForecastAsync(double latitude, double longitude)
        {
            Forecast result = await darkSkyProxy.GetTimeMachineWeatherAsync(latitude, longitude, DateTime.Now, Unit.Auto);
            List<HourDataPoint> hourDataPoints =  result.Hourly.Hours.ToList();

            DERMSCommon.WeatherForecast.WeatherForecast weatherForecast = new DERMSCommon.WeatherForecast.WeatherForecast(1001,1, 1, 1, 1, DateTime.Now, "");

            return weatherForecast;
        }
    }
}
