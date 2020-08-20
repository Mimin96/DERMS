using DERMSCommon.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    public interface IScadaCloudServer
    {
        Dictionary<List<long>, ushort> SendAnalogAndDigitalSignals(Dictionary<long, IdentifiedObject> analogni, Dictionary<long, IdentifiedObject> digitalni);
        void SendCommandToSimlator(ushort length, byte functionCode, ushort outputAddress, ushort value);
    }
}
