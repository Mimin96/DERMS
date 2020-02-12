using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
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

        public ConsumptionCalculator(NetworkModelTransfer networkModel)
        {
            this.networkModel = networkModel;
        }

    }
}
