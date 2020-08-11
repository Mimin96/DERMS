using CalculationEngineServiceCommon;
using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIClientFlexibilityFromUIToCE : ClientBase<IFlexibilityFromUIToCE>, IFlexibilityFromUIToCE
    {
        public UIClientFlexibilityFromUIToCE()
        {

        }

        public UIClientFlexibilityFromUIToCE(string endpoint) : base(endpoint)
        {

        }
        public Task ChangeBreakerStatus(long GID, bool NormalOpen)
        {
            return Channel.ChangeBreakerStatus(GID, NormalOpen);
        }

        public Task UpdateFlexibilityFromUIToCE(double valueKW, Enums.FlexibilityIncDec incOrDec, long gid)
        {
            return Channel.UpdateFlexibilityFromUIToCE(valueKW, incOrDec, gid);
        }
    }
}
