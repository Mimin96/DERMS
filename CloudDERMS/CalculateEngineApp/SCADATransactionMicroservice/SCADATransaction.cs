using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADATransactionMicroservice
{
    public class SCADATransaction : ITransactionCheck
    {
        ISendDataFromNMSToScada _scada;
        SignalsTransfer _st = null;
        public SCADATransaction() 
        {
            //this._scada = new SendDataFromNMSToScada();
        }
        public SCADATransaction(ISendDataFromNMSToScada scada)
        {
            this._scada = scada;
        }
        public async Task Commit()
        {
            //await _scada.SendGids(_st);
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
