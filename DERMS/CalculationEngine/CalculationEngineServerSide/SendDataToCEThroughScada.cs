using CalculationEngineServiceCommon;
using DERMSCommon;
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
            //smartCache.WriteToFile(data);
            //CAKI
            CalculationEngineCache.Instance.UpdateGraphWithScadaValues(data);

            CalculationEngineCache.Instance.UpdateNewDataPoitns(data);

            PubSubCalculatioEngine.Instance.Notify(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData);
            PubSubCalculatioEngine.Instance.Notify(data, (int)Enums.Topics.DataPoints);
            //ClientSideCE.Instance.ProxyUI_NM.SendDataUI(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass);
            //ClientSideCE.Instance.ProxyUI.SendScadaDataToUI(data);
        }
    }
}
