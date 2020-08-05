using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.Communication
{
    public class CommunicationWithTM
    {
        private ChannelFactory<ITransactionListing> factory;
        public ITransactionListing sendToTM;
        public CommunicationWithTM()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factory = new ChannelFactory<ITransactionListing>(binding, new EndpointAddress("net.tcp://localhost:18505/ITransactionListing"));
        }

        public void Open()
        {
            sendToTM = factory.CreateChannel();

        }
    }
}
