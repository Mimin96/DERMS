using DERMSCommon;
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
    public class SendDataFromNMSToCE : ISendDataFromNMSToCE
    {
        private static NetworkModelTransfer _nmt = null;
        public NetworkModelTransfer Nmt { get => _nmt; set => _nmt = value; }
        public bool CheckForTM(NetworkModelTransfer networkModel)
        {
            Nmt = networkModel;
            if (networkModel != null)
                return true;
            else
                return false;
        }

        public bool SendNetworkModel(NetworkModelTransfer networkModel)
        {
            // TREBA DPDATI U NetworkModelTransfer JEDNO POLJE GDE SE PROVERAVA DA LI JE MODEL UPDATE U PITANJU ILI JE NMS PROSLEDIO CEO MODEL PRILIKOM POKRETANJA APLIKACIJE
            // U ZAVISNOSTI OD TOGA TREBA POZVATI SLEDECE METODE:
            // AKO JE U PITANJU MODEL UPDATE POZIVA SE CalculationEngineCache.Instance.RestartCache(networkModel);
            // AKO JE U PITANJU CITAV MODEL PRILIKOM POKRETANJA APLIKACIJE POZIVA SE CalculationEngineCache.Instance.PopulateNSMModelCache(networkModel);
            networkModel = Nmt;
            if (networkModel.InitState)
                CalculationEngineCache.Instance.PopulateNSMModelCache(networkModel);
            else
                CalculationEngineCache.Instance.RestartCache(networkModel);
            //CalculationEngineCache.Instance.PopulateNSMModelCache(networkModel);
            CalculationEngineCache.Instance.PopulateWeatherForecast(networkModel);
            //ProductionCalculator productionCalculator = new ProductionCalculator(networkModel);
            //Dictionary<long, DerForecastDayAhead> substationsForecast = new Dictionary<long, DerForecastDayAhead>();

            //substationsForecast = productionCalculator.CalculateSubstations(CalculationEngineCache.Instance.GetForecast(4294967297));
            CalculationEngineCache.Instance.PopulateProductionForecast(networkModel);////////////////////
            CalculationEngineCache.Instance.PopulateConsumptionForecast(networkModel);
            // POZIV FLEXIBILITYA RADI TESTA
            //CalculationEngineCache.Instance.PopulateFlexibility(networkModel); 

            //ClientSideCE.Instance.ProxyUI_NM.SendDataUI(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass);
            PubSubCalculatioEngine.Instance.Notify(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass,(int)Enums.Topics.NetworkModelTreeClass_NodeData);
            if (networkModel != null)
                return true;
            else
                return false;
        }
    }
}
