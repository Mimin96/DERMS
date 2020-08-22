using DERMSCommon.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    [ServiceContract]
    public interface IScadaCloudServer
    {
        [OperationContract]
        Dictionary<List<long>, ushort> SendAnalogAndDigitalSignals(Dictionary<long, IdentifiedObject> analogni, Dictionary<long, IdentifiedObject> digitalni);
        [OperationContract]
        void SendCommandToSimlator(ushort length, byte functionCode, ushort outputAddress, ushort value);
    }
}
