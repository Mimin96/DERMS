using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class CETransaction : ITransactionCheck
    {
        ISendDataFromNMSToCE _calcEngine;
        NetworkModelTransfer _nmt = null;
        public CETransaction(ISendDataFromNMSToCE calEngine)
        {
            this._calcEngine = calEngine;
        }
        public void Commit()
        {
            _calcEngine.SendNetworkModel(_nmt);
        }

        public bool Prepare()
        {
            return true;
        }

        public void Rollback()
        {
            return;
        }
    }
}
