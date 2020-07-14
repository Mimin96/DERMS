using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using FTN.Services.NetworkModelService;
using System.ServiceModel.Description;
using FTN.Common;
using DERMSCommon.TransactionManager;

namespace FTN.Services.NetworkModelService
{
    public class NetworkModelService : IDisposable
    {
        private NetworkModelDeepCopy nm = null;
        private List<ServiceHost> hosts = null;

        public NetworkModelService()
        {
            nm = new NetworkModelDeepCopy();
            GenericDataAccess.NetworkModelDeepCopy = nm;
            ResourceIterator.NetworkModelDeepCopy = nm;
            InitializeHosts();
        }

        public void Start()
        {
            StartHosts();
            nm.StartService();
        }

        public void Dispose()
        {
            CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>();
            hosts.Add(new ServiceHost(typeof(GenericDataAccess)));
            hosts.Add(StartNmsTCSrv());
        }

        public ServiceHost StartNmsTCSrv()
        {
            ServiceHost serviceHost = new ServiceHost(new NMSTransaction(nm));
            var behaviour = serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };

            string address = String.Format("net.tcp://localhost:19506/ITransactionCheck");
            serviceHost.AddServiceEndpoint(typeof(ITransactionCheck), binding, address);
            return serviceHost;
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Services can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                message = "Endpoints:";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
                }

                Console.WriteLine("\n");
            }

            message = string.Format("Connection string: {0}", Config.Instance.ConnectionString);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);


            message = "The Network Model Service is started.";
            Console.WriteLine("\n{0}", message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
        }

        public void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("Network Model Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The Network Model Service is closed.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
