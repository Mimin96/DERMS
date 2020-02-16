using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.Configuration
{
    class SCADATranscation : ITransactionCheck
    {
        ISendDataFromNMSToScada _scada;
        SignalsTransfer _st = null;
        public SCADATranscation(ISendDataFromNMSToScada scada)
        {
            this._scada = scada;
        }
        public void Commit()
        {
            _scada.SendGids(_st);
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
