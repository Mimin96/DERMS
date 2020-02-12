using CalculationEngineService;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecast;
namespace CalculationEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            //DarkSkyApi darkSkyApi = new DarkSkyApi();
            //darkSkyApi.GetWeatherForecastAsync(45.25167, 19.83694);

            ServiceManager serviceManager = new ServiceManager();
            ClientSideCE n = ClientSideCE.Instance;

            n.Connect();

            Console.ReadLine();
        }
    }
}
