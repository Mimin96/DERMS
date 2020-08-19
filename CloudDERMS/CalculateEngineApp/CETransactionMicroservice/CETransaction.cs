using CECacheMicroservice;
using CloudCommon.CalculateEngine;
using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CETransactionMicroservice
{
    public class CETransaction : ITransactionCheck
    {
        DERMSCommon.NMSCommuication.ISendDataFromNMSToCE _calcEngine;
        DERMSCommon.NMSCommuication.NetworkModelTransfer _nmt = null;
        public CETransaction()
        {
            //this._calcEngine = new SendDataFromNMSToCE();
        }
        public CETransaction(DERMSCommon.NMSCommuication.ISendDataFromNMSToCE calEngine)
        {
            this._calcEngine = calEngine;
        }
        public async Task Commit()
        {
            CloudClient<ISendDataFromNMSToCE> nmsToCE = new CloudClient<ISendDataFromNMSToCE>
            (
                serviceUri: new Uri($"fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CESendDataFromNMSListener"
            );

            await nmsToCE.InvokeWithRetryAsync(client => client.Channel.SendNetworkModel(_nmt));

            //await _calcEngine.SendNetworkModel(_nmt);
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
