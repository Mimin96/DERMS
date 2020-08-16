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
    public class SendDataFromNMSToCE : CloudCommon.CalculateEngine.ISendDataFromNMSToCE
    {
        private IReliableStateManager _stateManager;
        private ICache _cache;

        public SendDataFromNMSToCE(IReliableStateManager stateManager, ICache cache) 
        {
            _stateManager = stateManager;
            _cache = cache;
        }



        public bool CheckForTM(NetworkModelTransfer networkModel)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<NetworkModelTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("networkModelTransfer").Result;

                NetworkModelTransfer modelTransfer = queue.TryDequeueAsync(tx).Result.Value;
                queue.EnqueueAsync(tx, networkModel);

                tx.CommitAsync();
            }

            if (networkModel != null)
                return true;
            else
                return false;
        }

        public bool SendNetworkModel(NetworkModelTransfer networkModel)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<NetworkModelTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("networkModelTransfer").Result;

                networkModel = queue.TryPeekAsync(tx).Result.Value;
            }

            if (networkModel.InitState)
                _cache.PopulateNSMModelCache(networkModel);
            else
                _cache.RestartCache(networkModel);

            _cache.PopulateWeatherForecast(networkModel);

            _cache.PopulateProductionForecast(networkModel);
            _cache.PopulateConsumptionForecast(networkModel);

            // pozvati pubSub na ovom mestu
            //PubSubCalculatioEngine.Instance.Notify(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData);
            
            if (networkModel != null)
                return true;
            else
                return false;
        }
    }
}
