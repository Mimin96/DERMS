using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionCoordinator
{
    public class TranscationCoordinatorCheck : ITransactionCheck
    {
        private string clientAddress;
        private ITransactionCheck proxy = null;
        private ChannelFactory<ITransactionCheck> factory;

        public TranscationCoordinatorCheck(string clientAddress)
        {
            this.clientAddress = clientAddress;
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factory = new ChannelFactory<ITransactionCheck>(binding, clientAddress);			
            proxy = factory.CreateChannel();
			((IContextChannel)proxy).OperationTimeout = new TimeSpan(0, 5, 0);
		}

		public void Commit()
        {
            proxy.Commit();
        }

        public bool Prepare()
        {
            return proxy.Prepare();
        }

        public void Rollback()
        {
            proxy.Rollback();
        }
    }
}
