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
        }

        public void StopServices()
        {
            serviceHostUI.Close();
            serviceHostScada.Close();
        }
    }
}
