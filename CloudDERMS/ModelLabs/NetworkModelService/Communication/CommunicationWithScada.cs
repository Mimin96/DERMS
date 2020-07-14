using DERMSCommon.NMSCommuication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.Communication
{
    public class CommunicationWithScada
    {
        private ChannelFactory<ISendDataFromNMSToScada> factory;
        public ISendDataFromNMSToScada sendToScada;
        public CommunicationWithScada()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factory = new ChannelFactory<ISendDataFromNMSToScada>(binding, new EndpointAddress("net.tcp://localhost:19012/ISendDataFromNMSToScada"));
        }

        public void Open()
        {
            sendToScada = factory.CreateChannel();

        }
    }
}
