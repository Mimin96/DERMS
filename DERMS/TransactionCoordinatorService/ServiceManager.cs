using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionCoordinator;

namespace TransactionCoordinatorService
{
    public class ServiceManager
    {
        private ServiceHost serviceHostUI;
        private ServiceHost serviceHostScada;
        private ServiceHost serviceHostForNMS;
        private ServiceHost serviceHostForCE;

        public ServiceManager()
        {
            try
            {
                StartService();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public void StartService()
        {           
            //Open service for NMS
            string address = String.Format("net.tcp://localhost:18505/ITransactionListing");
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForNMS = new ServiceHost(typeof(TransactionCoordinator.TransactionCoordinator));

            serviceHostForNMS.AddServiceEndpoint(typeof(ITransactionListing), binding, address);
            serviceHostForNMS.Open();
            Console.WriteLine("Open: net.tcp://localhost:18505/ITransactionListing");

            // Open service for CE 
            string address1 = String.Format("net.tcp://localhost:20505/ITransactionListing");
            NetTcpBinding binding1 = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForCE = new ServiceHost(typeof(TransactionCoordinator.TransactionCoordinator));

            serviceHostForCE.AddServiceEndpoint(typeof(ITransactionListing), binding1, address1);
            serviceHostForCE.Open();
            Console.WriteLine("Open: net.tcp://localhost:20505/ITransactionListing");

            // Open service for SCADA 
            string address2 = String.Format("net.tcp://localhost:20508/ITransactionListing");
            NetTcpBinding binding2 = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostScada = new ServiceHost(typeof(TransactionCoordinator.TransactionCoordinator));

            serviceHostScada.AddServiceEndpoint(typeof(ITransactionListing), binding2, address2);
            serviceHostScada.Open();
            Console.WriteLine("Open: net.tcp://localhost:20508/ITransactionListing");

        }

        public void StopServices()
        {
            serviceHostUI.Close();
            serviceHostScada.Close();
        }
    }
}
