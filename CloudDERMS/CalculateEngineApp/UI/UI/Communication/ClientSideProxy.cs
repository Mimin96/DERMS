using CalculationEngineServiceCommon;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;

namespace UI.Communication
{
    public class ClientSideProxy
    {
        public string ClientAddress { get; set; }
        private ChannelFactory<IPubSubCalculateEngine> factory;
        private ServiceHost ServiceHost { get; set; }
        private static CalculationEnginePubSub _calculationEnginePubSub;

        private CloudClient<IPubSub> transactionCoordinator;

        private static ClientSideProxy instance = null;
        public static ClientSideProxy Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ClientSideProxy();

                    _calculationEnginePubSub = new CalculationEnginePubSub();
                    instance.StartServiceHost(_calculationEnginePubSub);
                }

                return instance;

            }
        }

        public async void Subscribe(int gidOfTopic)
        {
            bool ret = false;

            while (!ret) {
                try
                {
                    ret = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SubscribeSubscriber(ClientAddress, gidOfTopic));
                }
                catch
                {

                }
            }
        }

        public void StartServiceHost(ICalculationEnginePubSub observerInstance)
        {
            string ipAddress = GetLocalIPAddress();
            int port = GetAvailablePort();

            ClientAddress = String.Format("net.tcp://{0}:{1}/ICECommunicationPubSub", ipAddress, port);

            transactionCoordinator = new CloudClient<IPubSub>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEPubSubMicroServiceListener"
            );

            ServiceHost = new ServiceHost(observerInstance);
            var behaviour = ServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost.AddServiceEndpoint(typeof(ICalculationEnginePubSub), binding, ClientAddress);
            ServiceHost.Open();
        }

        public void Close()
        {
            factory.Close();
            ServiceHost.Close();
        }

        private int GetAvailablePort()
        {
            int startingPort = 20000;
            var portArray = new List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
            {
                if (!portArray.Contains(i))
                {
                    return i;
                }
            }

            return -1;

        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress.ToString();
        }
    }
}
