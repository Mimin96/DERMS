﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CalculationEngineService;
using CalculationEngineServiceCommon;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace CECommandMicroservice
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CECommandMicroservice : StatelessService
    {
        public CECommandMicroservice(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var ip = Context.NodeContext.IPAddressOrFQDN;

            CEUpdateThroughUIService tcs = new CEUpdateThroughUIService();
            tcs.MessageRcv += (sender, s) => MessageForDiagnosticEvents(s);

            CEUpdateThroughUIService tcs2 = new CEUpdateThroughUIService();
            tcs2.MessageRcv += (sender, s) => MessageForDiagnosticEvents(s);

            FlexibilityFromUIToCEService tcfs = new FlexibilityFromUIToCEService();
            tcfs.MessageRcv += (sender, s) => MessageForDiagnosticEvents(s);

            return new[]
            {
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<ICEUpdateThroughUI>(
                        wcfServiceObject: tcs,
                        serviceContext: context,
                        endpointResourceName: "CEUpdateThroughUIServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "CEUpdateThroughUIServiceListener"
                ),
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<ICEUpdateThroughUI>(
                        wcfServiceObject: tcs2,
                        serviceContext: context,
                        address: new EndpointAddress("net.tcp://localhost:55556/CECommandMicroservice"),
                        listenerBinding: new NetTcpBinding()
                    ),
                    name: "CECommandMicroserviceListener"
                ),
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<IFlexibilityFromUIToCE>(
                        wcfServiceObject: tcfs,
                        serviceContext: context,
                        address: new EndpointAddress("net.tcp://localhost:8080/CECommandMicroservice"),
                        listenerBinding: new NetTcpBinding()
                    ),
                    name: "FlexibilityFromUIToCEServiceListener"
                )
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;
            ServiceEventSource.Current.Message("CECommandMicroservice, Up and running.");
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

               // ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private void MessageForDiagnosticEvents(string message)
        {
            ServiceEventSource.Current.Message(message);
        }
}
}
