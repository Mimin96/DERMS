using CalculationEngineServiceCommon;
using DERMSCommon;
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
        public void SendDataUI(TreeNode<NodeData> data)
        {
            Mediator.NotifyColleagues("NMSNetworkModelData", data);
        }
    }
}
