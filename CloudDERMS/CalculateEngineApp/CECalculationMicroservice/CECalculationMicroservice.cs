using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon.CalculateEngine;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.Fabric.Description;
using CalculationEngineService;

namespace CECalculationMicroservice
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CECalculationMicroservice : StatelessService
    {
        public CECalculationMicroservice(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var ip = Context.NodeContext.IPAddressOrFQDN;

            return new[]
            {
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<IConsumptionCalculator>(
                        wcfServiceObject: new ConsumptionCalculatorService(),
                        serviceContext: context,
                        endpointResourceName: "ConsumptionCalculatorEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "ConsumptionCalculatorListener"
                ),
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<IProductionCalculator>(
                        wcfServiceObject: new ProductionCalculatorService(),
                        serviceContext: context,
                        endpointResourceName: "ProductionCalculatorEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "ProductionCalculatorListener"
                ),
                 new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<IIslandCalculations>(
                        wcfServiceObject: new IslandCalculationsService(),
                        serviceContext: context,
                        endpointResourceName: "IslandCalculationsEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "IslandCalculationsListener"
                ),
                 new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<IDERFlexibility>(
                        wcfServiceObject: new DERFlexibility(),
                        serviceContext: context,
                        endpointResourceName: "DERFlexibilityEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "DERFlexibilityListener"
                ),
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
            ServiceEventSource.Current.Message("CECalculationMicroservice, Up and running.");
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
