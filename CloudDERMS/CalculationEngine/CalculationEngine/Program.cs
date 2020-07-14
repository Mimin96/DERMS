using CalculationEngineService;
using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
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

            PubSubCalculatioEngine pubSubCalculatioEngine = new PubSubCalculatioEngine();

            ServiceManager serviceManager = new ServiceManager(PubSubCalculatioEngine.Instance);

            ClientSideCE n = ClientSideCE.Instance;

            n.Connect();

            n.ProxyTM.Enlist("net.tcp://localhost:19516/ITransactionCheck");
            /*CalculationEngineCache.Instance.AddDerForecast(new DerForecastDayAhead(), 1, false);
            CalculationEngineCache.Instance.AddDerForecast(new DerForecastDayAhead(), 2, false);
            CalculationEngineCache.Instance.AddDerForecast(new DerForecastDayAhead(), 3, false);
            CalculationEngineCache.Instance.AddDerForecast(new DerForecastDayAhead(), 4, false);*/
                       
            Console.WriteLine("Press enter to send data.");
            Console.ReadLine();

            DataToUI data = new DataToUI();
            data.Data = CalculationEngineCache.Instance.GetAllDerForecastDayAhead();
            PubSubCalculatioEngine.Instance.Notify(data, (int)Enums.Topics.Flexibility);

            Console.WriteLine("Press enter to exit.");

            Console.ReadLine();
        }
    }
}
