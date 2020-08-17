using CloudCommon.CalculateEngine;
using DERMSCommon.NMSCommuication;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
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
        private NetworkModelTransfer _nmt;

        public NetworkModelTransfer Nmt { get => _nmt; set => _nmt = value; }

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
                queue.EnqueueAsync(tx, networkModel);

                await tx.CommitAsync();
            }

            Nmt = networkModel;

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

            networkModel = Nmt;
            if (networkModel != null)            
                networkModel.InitState = true;            

            if (networkModel.InitState)
                _cache.PopulateNSMModelCache(networkModel);
            else
                _cache.RestartCache(networkModel);            

            // pozvati pubSub na ovom mestu
            //PubSubCalculatioEngine.Instance.Notify(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData);
            
            if (networkModel.Insert.Count != 0)
            {
                _cache.PopulateWeatherForecast(networkModel);

                _cache.PopulateProductionForecast(networkModel);
                _cache.PopulateConsumptionForecast(networkModel);

                return true;
            }                
            else
                return false;
        }
    }
}
