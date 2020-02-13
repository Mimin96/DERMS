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
        public bool SendGids(SignalsTransfer signals)
        {
            if (signals != null)
                return true;
            else
                return false;
        }
    }
}
