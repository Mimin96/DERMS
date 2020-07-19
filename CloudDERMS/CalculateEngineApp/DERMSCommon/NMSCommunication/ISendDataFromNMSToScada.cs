
using DERMSCommon.DataModel.Meas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.NMSCommuication
{
    [ServiceContract]
    public interface ISendDataFromNMSToScada
    {
        [OperationContract]
        [ServiceKnownType(typeof(Measurement))]
        [ServiceKnownType(typeof(Discrete))]
        [ServiceKnownType(typeof(Analog))]
        bool SendGids(SignalsTransfer signals);

        [OperationContract]
        [ServiceKnownType(typeof(Measurement))]
        [ServiceKnownType(typeof(Discrete))]
        [ServiceKnownType(typeof(Analog))]
        bool CheckForTM(SignalsTransfer signals);

    }
}
