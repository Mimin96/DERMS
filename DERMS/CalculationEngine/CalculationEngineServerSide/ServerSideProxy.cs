using CalculationEngineServiceCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    // OVA KLASA SE KORISTI ZA PUBSUB TO CE KASNIJE BITI IMPLEMENTIRANO, ZA SAD SU PIPELINE SAMO KLASE ServiceManager i ClientSideCE
    public class ServerSideProxy
    {
        public ChannelFactory<ISendSCADADataToUI> factory;
        public ISendSCADADataToUI Proxy { get; set; }
        public string ClientAddress { get; set; }
        public ServerSideProxy(string clientAddress)
        {
            this.ClientAddress = clientAddress;
            Connect();
        }

        public void Connect()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factory = new ChannelFactory<ISendSCADADataToUI>(binding, new EndpointAddress(ClientAddress));
            Proxy = factory.CreateChannel();
        }

        public void Abort()
        {
            factory.Abort();
        }

        public void Close()
        {
            factory.Close();
        }
    }
}
