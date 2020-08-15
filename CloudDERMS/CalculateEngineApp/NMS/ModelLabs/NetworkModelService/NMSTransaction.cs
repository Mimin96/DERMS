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
        public async Task Commit()
        {
            networkModelDeepCopy.Commit();
        }

        public async Task<bool> Prepare()
        {
            return true;
        }

        public async Task Rollback()
        {
            networkModelDeepCopy.Rollback();
        }
    }
}
