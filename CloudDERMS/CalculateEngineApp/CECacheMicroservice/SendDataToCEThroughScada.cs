using CalculationEngineServiceCommon;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon;
using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
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
            await _cache.UpdateGraphWithScadaValues(data);

            _cache.UpdateNewDataPoitns(data).Wait();

            CloudClient<IPubSub> pubSub = new CloudClient<IPubSub>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEPubSubMicroServiceListener"
            );

            TreeNode<NodeData> dataa = _cache.GetGraph().Result;
            List<DERMSCommon.UIModel.ThreeViewModel.NetworkModelTreeClass> NetworkModelTreeClass = _cache.GetNetworkModelTreeClass().Result;
            await pubSub.InvokeWithRetryAsync(client => client.Channel.NotifyTree(dataa, NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData));
            await pubSub.InvokeWithRetryAsync(client => client.Channel.NotifyDataPoint(data, (int)Enums.Topics.DataPoints));

            //ClientSideCE.Instance.ProxyUI_NM.SendDataUI(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass);
            //ClientSideCE.Instance.ProxyUI.SendScadaDataToUI(data);
        }

    }
}
