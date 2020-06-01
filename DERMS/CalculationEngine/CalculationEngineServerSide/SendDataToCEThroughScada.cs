﻿using CalculationEngineServiceCommon;
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
            ClientSideCE.Instance.ProxyUI_NM.SendDataUI(CalculationEngineCache.Instance.GraphCached, CalculationEngineCache.Instance.NetworkModelTreeClass);
            ClientSideCE.Instance.ProxyUI.SendScadaDataToUI(data);
        }
    }
}
