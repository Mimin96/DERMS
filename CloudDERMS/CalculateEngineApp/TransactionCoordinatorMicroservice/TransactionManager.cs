using DERMSCommon.TransactionManager;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionCoordinatorMicroservice
{
    public class TransactionManager : ITransactionListing
    {
        private static List<string> _activeServices = new List<string>();
        bool result = false;

        public TransactionManager()
        {
        }

        private async Task Rollback()
        {
            CloudClient<ITransactionCheck> nmsClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/NMSApp/NMSTransactionMicroservice"),
               partitionKey:  ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "NMSTMListener"
            );

            await nmsClient.InvokeWithRetryAsync(client => client.Channel.Rollback());

            CloudClient<ITransactionCheck> ceClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/CalculateEngineApp/CETransactionMicroservice"),
               partitionKey: ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "CETMListener"
            );

            await ceClient.InvokeWithRetryAsync(client => client.Channel.Rollback());

            CloudClient<ITransactionCheck> scadaClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/SCADAApp/SCADATransactionMicroservice"),
               partitionKey: ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "SCADATMListener"
            );

            await scadaClient.InvokeWithRetryAsync(client => client.Channel.Rollback());
        }

        private async Task Commit()
        {
            CloudClient<ITransactionCheck> nmsClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/NMSApp/NMSTransactionMicroservice"),
               partitionKey: ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "NMSTMListener"
            );

            await nmsClient.InvokeWithRetryAsync(client => client.Channel.Commit());

            CloudClient<ITransactionCheck> ceClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/CalculateEngineApp/CETransactionMicroservice"),
               partitionKey: ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "CETMListener"
            );

            await ceClient.InvokeWithRetryAsync(client => client.Channel.Commit());

            CloudClient<ITransactionCheck> scadaClient = new CloudClient<ITransactionCheck>
            (
               serviceUri: new Uri($"fabric:/SCADAApp/SCADATransactionMicroservice"),
               partitionKey: ServicePartitionKey.Singleton,
               clientBinding: WcfUtility.CreateTcpClientBinding(),
               listenerName: "SCADATMListener"
            );

            await scadaClient.InvokeWithRetryAsync(client => client.Channel.Commit());
        }

        public async Task Enlist(string adress)
        {
            _activeServices.Add(adress);
        }

        public async Task FinishList(bool IsSuccessfull)
        {
            if (IsSuccessfull)
            {
                bool prepareSuccessfull = true;

                CloudClient<ITransactionCheck> nmsClient = new CloudClient<ITransactionCheck>
                (
                   serviceUri: new Uri($"fabric:/NMSApp/NMSTransactionMicroservice"),
                   partitionKey: ServicePartitionKey.Singleton,
                   clientBinding: WcfUtility.CreateTcpClientBinding(),
                   listenerName: "NMSTMListener"
                );

                bool result = await nmsClient.InvokeWithRetryAsync(client => client.Channel.Prepare());

                if (result == false)
                {
                    prepareSuccessfull = false;
                }

                if (!prepareSuccessfull)
                {
                    await Rollback();
                }
                else
                {
                    await Commit();
                }
            }
        }
    }
}
