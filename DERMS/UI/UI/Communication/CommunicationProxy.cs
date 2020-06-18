using CalculationEngineServiceCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class CommunicationProxy
    {
        //private ServiceHost serviceHost_SCADAData;
        //private ServiceHost serviceHost_NetworkModel;
       // private ServiceHost serviceHost_Events;
        private ChannelFactory<ICEUpdateThroughUI> factory;
        public ICEUpdateThroughUI sendToCE;

        public CommunicationProxy()
        {
            //Receive from CE
            //serviceHost_SCADAData = new ServiceHost(typeof(SendSCADADataToUI));
            //NetTcpBinding binding = new NetTcpBinding();
            //binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            //serviceHost_SCADAData.AddServiceEndpoint(typeof(ISendSCADADataToUI), binding,
            //                                new Uri("net.tcp://localhost:29139/ISendSCADADataToUI"));

            // Receive from CE
            //serviceHost_NetworkModel = new ServiceHost(typeof(SendNetworkModelToUI));

            //serviceHost_NetworkModel.AddServiceEndpoint(typeof(ISendNetworkModelToUI), new NetTcpBinding(),
            //                                new Uri("net.tcp://localhost:27138/ISendNetworkModelToUI"));

             // Receive from CE
            // serviceHost_Events = new ServiceHost(typeof(SendEventsToUI));

            // serviceHost_Events.AddServiceEndpoint(typeof(ISendEventsToUI), new NetTcpBinding(),
                                            // new Uri("net.tcp://localhost:27777/ISendEventsToUI"));

            // Send to CE
            factory = new ChannelFactory<ICEUpdateThroughUI>(new NetTcpBinding(),
                                                                    new EndpointAddress("net.tcp://localhost:19001/ICEUpdateThroughUI"));
        }

        public void Open()
        {
           // serviceHost_SCADAData.Open();
           // serviceHost_NetworkModel.Open();
           // sendToCE = factory.CreateChannel();
            // serviceHost_Events.Open();

        }
        public void Open2()
        {
            sendToCE = factory.CreateChannel();
        }

        public void Close()
        {
           // serviceHost_SCADAData.Close();
           // serviceHost_NetworkModel.Close();
            // serviceHost_Events.Close();
        }
    }
}
