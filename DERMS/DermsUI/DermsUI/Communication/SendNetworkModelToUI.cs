using CalculationEngineServiceCommon;
using DERMSCommon.NMSCommuication;
using DermsUI.MediatorPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Communication
{
    public class SendNetworkModelToUI : ISendNetworkModelToUI
    {
        public void SendDataUI(NetworkModelTransfer data)
        {
            Mediator.NotifyColleagues("NMSNetworkModelData", data);
        }
    }
}
