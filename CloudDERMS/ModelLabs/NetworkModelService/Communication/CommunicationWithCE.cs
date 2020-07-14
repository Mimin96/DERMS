using DERMSCommon.NMSCommuication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.Communication
{
    public class CommunicationWithCE
    {
        private ChannelFactory<ISendDataFromNMSToCE> factory;
        public ISendDataFromNMSToCE sendToCE;
        public CommunicationWithCE()
        {
            factory = new ChannelFactory<ISendDataFromNMSToCE>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:19002/ISendDataFromNMSToCE"));
        }

        public void Open()
        {
            sendToCE = factory.CreateChannel();

        }
    }
}
