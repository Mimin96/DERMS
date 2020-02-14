using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public interface INotify
    {
        void Notify(Dictionary<long, DerForecastDayAhead> forcastDayAhead, int gidOfTopic);
    }
}
