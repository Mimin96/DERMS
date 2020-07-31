using DarkSkyApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
    [ServiceContract]
    public interface IDarkSkyApi
    {
        [OperationContract]
        Task<Forecast> GetWeatherForecastAsync(double latitude, double longitude);
    }
}
