using DERMSCommon.NMSCommuication;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class SendDataFromNMSToCE : ISendDataFromNMSToCE
    {
        public void SendNetworkModel(NetworkModelTransfer networkModel)
        {
            // TREBA DPDATI U NetworkModelTransfer JEDNO POLJE GDE SE PROVERAVA DA LI JE MODEL UPDATE U PITANJU ILI JE NMS PROSLEDIO CEO MODEL PRILIKOM POKRETANJA APLIKACIJE
            // U ZAVISNOSTI OD TOGA TREBA POZVATI SLEDECE METODE:
            // AKO JE U PITANJU MODEL UPDATE POZIVA SE CalculationEngineCache.Instance.RestartCache(networkModel);
            // AKO JE U PITANJU CITAV MODEL PRILIKOM POKRETANJA APLIKACIJE POZIVA SE CalculationEngineCache.Instance.PopulateNSMModelCache(networkModel);
            return;
        }
    }
}
