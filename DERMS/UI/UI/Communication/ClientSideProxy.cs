using CalculationEngineServiceCommon;
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

namespace UI.Communication
{
    public class ClientSideProxy
    {
        private string clientAddress;
        private IPubSubCalculateEngine proxy = null;
        private ChannelFactory<IPubSubCalculateEngine> factory;
        private ServiceHost serviceHost;
        private static CalculationEnginePubSub _calculationEnginePubSub;

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

        private ClientSideProxy()
        {
            string ipAddress = GetLocalIPAddress();
            int port = GetAvailablePort();

            clientAddress = String.Format("net.tcp://{0}:{1}/ICalculationEnginePubSub", ipAddress, port);
            ConnectToService();
        }

        public void StartServiceHost(ICalculationEnginePubSub observerInstance)
        {
            serviceHost = new ServiceHost(observerInstance);
            var behaviour = serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHost.AddServiceEndpoint(typeof(ICalculationEnginePubSub), binding, clientAddress);
            serviceHost.Open();
        }

        public void Subscribe(int gidOfTopic)
        {
            try
            {
                proxy.Subscribe(clientAddress, gidOfTopic);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Unsubscribe(int gidOfTopic, bool disconnect)
        {
            try
            {
                proxy.Unsubscribe(clientAddress, gidOfTopic, disconnect);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectToService()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factory = new ChannelFactory<IPubSubCalculateEngine>(binding, new EndpointAddress("net.tcp://localhost:19000/IPubSubCalculateEngine"));
            proxy = factory.CreateChannel();
        }

        public void Close()
        {
            factory.Close();
            serviceHost.Close();
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
