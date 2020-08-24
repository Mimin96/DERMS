using CalculationEngineServiceCommon;
using CloudCommon.CalculateEngine;
using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECacheMicroservice
{
    public class SendDataToCEThroughScada : ISendDataToCEThroughScada
    {
        private IReliableStateManager _stateManager;
        private ICache _cache;


        public SendDataToCEThroughScada(IReliableStateManager stateManager, ICache cache)
        {
            _stateManager = stateManager;
            _cache = cache;
        }

        public async Task ReceiveFromScada(List<DataPoint> data)
        {
            _cache.UpdateGraphWithScadaValues(data);

            _cache.UpdateNewDataPoitns(data);
            //KOMUNIKACIJA SA PUB SUBOM ODRADITIT
            //PubSubCalculatioEngine.Instance.Notify(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData);
            // PubSubCalculatioEngine.Instance.Notify(data, (int)Enums.Topics.DataPoints);



            //ClientSideCE.Instance.ProxyUI_NM.SendDataUI(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass);
            //ClientSideCE.Instance.ProxyUI.SendScadaDataToUI(data);
        }

    }
}
