using DarkSkyApi.Models;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
    [ServiceContract]
    public interface IConsumptionCalculator
    {
        [OperationContract]
        void Calculate(Dictionary<long, DerForecastDayAhead> derForcast, NetworkModelTransfer networkModel, Dictionary<long, DayAhead> subDayAhead, Dictionary<long, Forecast> DerWeather);
    }
}
