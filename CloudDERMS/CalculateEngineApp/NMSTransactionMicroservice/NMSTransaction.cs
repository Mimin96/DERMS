using DERMSCommon.TransactionManager;
using FTN.Services.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSTransactionMicroservice
{
    public class NMSTransaction : ITransactionCheck
    {
        protected static NetworkModelDeepCopy networkModelDeepCopy = null;
        public NMSTransaction()
        {
            networkModelDeepCopy = new NetworkModelDeepCopy();
        }

        public static NetworkModelDeepCopy NetworkModelDeepCopy
        {
            set
            {
                networkModelDeepCopy = value;
            }
        }
        public async Task Commit()
        {
            await networkModelDeepCopy.Commit();
        }

        public async Task<bool> Prepare()
        {
            return true;
        }

        public async Task Rollback()
        {
            await networkModelDeepCopy.Rollback();
        }
    }
}
