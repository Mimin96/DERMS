using CalculationEngineServiceCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.SmartCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class SendDataToCEThroughScada : ISendDataToCEThroughScada
    {
        public void ReceiveFromScada(List<DataPoint> data)
        {
            SCADADataPointSmartCache smartCache = new SCADADataPointSmartCache();
            smartCache.WriteToFile(data);
            ClientSideCE.Instance.ProxyUI.SendDataUI(data);
        }
    }
}
