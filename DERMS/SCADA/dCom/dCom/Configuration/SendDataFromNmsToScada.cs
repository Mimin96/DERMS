using DERMSCommon.NMSCommuication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.Configuration
{
    public class SendDataFromNmsToScada : ISendDataFromNMSToScada
    {
        private static SignalsTransfer signalsTransfer;
        public static SignalsTransfer SignalsTransfer { get => signalsTransfer; set => signalsTransfer = value; }

        public bool CheckForTM (SignalsTransfer signals)
        {
            SignalsTransfer = signals;            
            if (signals != null)
                return true;
            else
                return false;
        }
        public bool SendGids(SignalsTransfer signals)
        {
            signals = SignalsTransfer;
            if (signals != null)
                return true;
            else
                return false;
        }
    }
}
