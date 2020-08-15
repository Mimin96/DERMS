using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.TransactionManager
{
    public class CloudClient<TServiceContract> : ServicePartitionClient<WcfCommunicationClient<TServiceContract>> where TServiceContract : class
    {
        public CloudClient(Uri serviceUri, Binding clientBinding, ServicePartitionKey partitionKey = null, string listenerName = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, OperationRetrySettings retrySettings = null)
            : base(
                  new WcfCommunicationClientFactory<TServiceContract>(clientBinding: clientBinding, servicePartitionResolver: ServicePartitionResolver.GetDefault()),
                  serviceUri,
                  partitionKey,
                  targetReplicaSelector,
                  listenerName,
                  retrySettings
              )
        {
        }
    }
}
