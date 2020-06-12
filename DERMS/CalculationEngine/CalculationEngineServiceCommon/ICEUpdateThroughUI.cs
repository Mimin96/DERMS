using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface ICEUpdateThroughUI
    {
        [OperationContract]
        float UpdateThroughUI(long data);
        [OperationContract]
        float BalanceNetworkModel();
        [OperationContract]
        List<long> AllGeoRegions();
        [OperationContract]
        bool AllowOptimization(long gid);
    }
}
