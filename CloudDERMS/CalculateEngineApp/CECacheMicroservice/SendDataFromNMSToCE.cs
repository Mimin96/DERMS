using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon;
using DERMSCommon.NMSCommuication;
using DERMSCommon.UIModel.ThreeViewModel;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECacheMicroservice
{
    public class SendDataFromNMSToCE : DERMSCommon.NMSCommuication.ISendDataFromNMSToCE
    {
        private IReliableStateManager _stateManager;
        private ICache _cache;
        // private NetworkModelTransfer _nmt;

        // public NetworkModelTransfer Nmt { get => _nmt; set => _nmt = value; }

        public SendDataFromNMSToCE(IReliableStateManager stateManager, ICache cache)
        {
            _stateManager = stateManager;
            _cache = cache;
        }

        public SendDataFromNMSToCE()
        {

        }


        public async Task<bool> CheckForTM(NetworkModelTransfer networkModel)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<NetworkModelTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("networkModelTransfer").Result;

                NetworkModelTransfer modelTransfer = queue.TryDequeueAsync(tx).Result.Value;
                await queue.EnqueueAsync(tx, networkModel);

                await tx.CommitAsync();
            }

            // Nmt = networkModel;

            if (networkModel != null)
                return true;
            else
                return false;
        }

        public async Task<bool> SendNetworkModel(NetworkModelTransfer networkModel)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<NetworkModelTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("networkModelTransfer").Result;

                networkModel = queue.TryPeekAsync(tx).Result.Value;
            }

            // networkModel = Nmt;
            if (networkModel != null)
                networkModel.InitState = true;

            if (networkModel.InitState)
                 await _cache.PopulateNSMModelCache(networkModel);
            else
                _cache.RestartCache(networkModel);

            CloudClient<IPubSub> pubSub = new CloudClient<IPubSub>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEPubSubMicroServiceListener"
            );

            TreeNode<NodeData> data = _cache.GetGraph().Result;
            List<NetworkModelTreeClass> NetworkModelTreeClass = _cache.GetNetworkModelTreeClass().Result;
            await pubSub.InvokeWithRetryAsync(client => client.Channel.NotifyTree(data, NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData));

            if (networkModel.Insert.Count != 0)
            {
                await _cache.PopulateWeatherForecast(networkModel);

                await _cache.PopulateProductionForecast(networkModel);
                await _cache.PopulateConsumptionForecast(networkModel);

                return true;
            }
            else
                return false;
        }
    }
}
