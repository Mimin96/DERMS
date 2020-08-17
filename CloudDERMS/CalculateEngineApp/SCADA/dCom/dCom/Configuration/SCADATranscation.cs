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
        public async Task Commit()
        {
            await _scada.SendGids(_st);
        }

        public async Task<bool> Prepare()
        {
            return true;
        }

        public async Task Rollback()
        {
            return;
        }
    }
}
