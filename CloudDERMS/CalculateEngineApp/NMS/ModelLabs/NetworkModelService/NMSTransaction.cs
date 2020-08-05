using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService
{
    public class NMSTransaction : ITransactionCheck
    {
        private INetworkModelDeepCopy networkModelDeepCopy;

        public NMSTransaction(INetworkModelDeepCopy networkModelDeepCopy)
        {
            this.networkModelDeepCopy = networkModelDeepCopy;
        }
        public void Commit()
        {
            networkModelDeepCopy.Commit();
        }

        public bool Prepare()
        {
            return true;
        }

        public void Rollback()
        {
            networkModelDeepCopy.Rollback();
        }
    }
}
