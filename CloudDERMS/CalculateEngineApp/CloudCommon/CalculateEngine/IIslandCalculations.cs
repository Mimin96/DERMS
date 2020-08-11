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
    public interface IIslandCalculations
    {
        [OperationContract]
        Task GeneratorOff(long generatorGid, Dictionary<long, DerForecastDayAhead> prod);
        [OperationContract]
        Task GeneratorOn(long generatorGid, Dictionary<long, DerForecastDayAhead> prod);
    }
}
