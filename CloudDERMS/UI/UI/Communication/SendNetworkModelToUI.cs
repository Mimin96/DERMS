using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.NMSCommuication;
using DERMSCommon.UIModel.ThreeViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources.MediatorPattern;

namespace UI.Communication
{
    public class SendNetworkModelToUI : ISendNetworkModelToUI
    {
        public void SendDataUI(TreeNode<NodeData> data, List<NetworkModelTreeClass> NetworkModelTreeClass)
        {
            List<object> obj = new List<object>() { data, NetworkModelTreeClass };
            Mediator.NotifyColleagues("NMSNetworkModelData", obj);
            Mediator.NotifyColleagues("NMSNetworkModelDataDERDashboard", obj);
            Mediator.NotifyColleagues("NMSNetworkModelDataNetworkModel", obj);
            Mediator.NotifyColleagues("NMSNetworkModelDataGIS", obj);
        }
    }
}
