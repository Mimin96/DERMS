using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using FTN.Common;
using FTN.ServiceContracts;
using FTN.Services.NetworkModelService;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace NMSGDAMicroservice
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class NMSGDAMicroservice : StatefulService
    {
        public NMSGDAMicroservice(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {            
            return new[] {
                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<INetworkModelGDAContract>(
                        wcfServiceObject: new GDA(),
                        serviceContext: context,
                        address: new EndpointAddress("net.tcp://localhost:55555/NetworkModelMicroService"),
                        listenerBinding: new NetTcpBinding()
                    ),
                    name : "NMSGDAListener"
                ),

                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<INetworkModelGDAContract>(
                        wcfServiceObject: new GDA(),
                        serviceContext: context,
                        endpointResourceName: "GDAEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "GDA"
                )
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {        
            try
            {        
                using (NetworkModelService nms = new NetworkModelService())
                {
                    nms.Start();
                    while (true)
                    { }                    
                }
            }
            catch (Exception ex)
            {
               
            }

        }
    }
}
