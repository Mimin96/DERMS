using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACacheMicroservice
{
     public class CloudScadaToLocalScada : IScadaCloudToScadaLocal
    {
        private IReliableStateManager _stateManager;

        public CloudScadaToLocalScada(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task <bool> SendEndpoints(string endpoint)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<string> queue = _stateManager.GetOrAddAsync<IReliableQueue<string>>("endpoints").Result;
                await queue.EnqueueAsync(tx, endpoint);
                await tx.CommitAsync();
            }
            return true;
        }
    }
}
