using DERMSCommon;
using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources;

namespace UI.ViewModel
{
    public class DERDashboardUserControlViewModel : BindableBase
    {
        #region Properties
        public TreeNode<NodeData> Tree { get; set; }
        #endregion
    }
}
