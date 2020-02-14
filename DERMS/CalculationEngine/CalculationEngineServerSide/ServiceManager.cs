using CalculationEngineServiceCommon;
using DERMSCommon.NMSCommuication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CalculationEngineService
{
    public class ServiceManager
    {
        private ServiceHost serviceHostUI;
        private ServiceHost serviceHostScada;
        private ServiceHost serviceHostForNMS;
        private ServiceHost serviceHostPubSubCE;

        public ServiceManager(IPubSubCalculateEngine pubSubCalculateEngine)
        {
            try
            {
                StartSubscriptionService(pubSubCalculateEngine);
                StartService();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public void StartService()
        {
            string address = String.Format("net.tcp://localhost:19999/ISendDataToCEThroughScada");
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostScada = new ServiceHost(typeof(SendDataToCEThroughScada));

            serviceHostScada.AddServiceEndpoint(typeof(ISendDataToCEThroughScada), binding, address);
            serviceHostScada.Open();
            Console.WriteLine("Open: net.tcp://localhost:19999/ISendDataToCEThroughScada");

            //Open service for UI
            string address2 = String.Format("net.tcp://localhost:19001/ICEUpdateThroughUI");
            NetTcpBinding binding2 = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostUI = new ServiceHost(typeof(CEUpdateThroughUI));
            serviceHostUI.AddServiceEndpoint(typeof(ICEUpdateThroughUI), binding2, address2);
            serviceHostUI.Open();
            Console.WriteLine("Open: net.tcp://localhost:19001/ICEUpdateThroughUI");

            //Open service for NMS
            string address3 = String.Format("net.tcp://localhost:19002/ISendDataFromNMSToCE");
            NetTcpBinding binding3 = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForNMS = new ServiceHost(typeof(SendDataFromNMSToCE));

            serviceHostForNMS.AddServiceEndpoint(typeof(ISendDataFromNMSToCE), binding3, address3);
            serviceHostForNMS.Open();
            Console.WriteLine("Open: net.tcp://localhost:19002/ISendDataFromNMSToCE");
        }

        private void StartSubscriptionService(IPubSubCalculateEngine subcriptionService)
        {
            serviceHostPubSubCE = new ServiceHost(subcriptionService);
            var behaviour = serviceHostPubSubCE.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            string address = String.Format("net.tcp://localhost:19000/IPubSubCalculateEngine");
            serviceHostPubSubCE.AddServiceEndpoint(typeof(IPubSubCalculateEngine), binding, address);
            serviceHostPubSubCE.Open();
            Console.WriteLine("CalculationEngineService listening at:");
            Console.WriteLine(address);
        }

        public void StopServices()
        {
            serviceHostUI.Close();
            serviceHostScada.Close();
        }
    }
}
