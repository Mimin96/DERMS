using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADATransactionMicroservice
{
    public class SCADATransaction : ITransactionCheck
    {
        ISendDataFromNMSToScada _scada;
        SignalsTransfer _st = null;
        public SCADATransaction() 
        {
            //this._scada = new SendDataFromNMSToScada();
        }
        public SCADATransaction(ISendDataFromNMSToScada scada)
        {
            this._scada = scada;
        }
        public async Task Commit()
        {
            CloudClient<ISendDataFromNMSToScada> nmsToScada = new CloudClient<ISendDataFromNMSToScada>
            (
               serviceUri: new Uri($"fabric:/SCADAApp/SCADACacheMicroservice"),
               partitionKey: new ServicePartitionKey(0),
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "SCADACacheMicroserviceListener"
            );

            await nmsToScada.InvokeWithRetryAsync(client => client.Channel.SendGids(_st));
            //await _scada.SendGids(_st);
        }

        public async Task<bool> Prepare()
        {
            return true;
        }

        public async Task Rollback()
        {
            return;
        }
    }
}
