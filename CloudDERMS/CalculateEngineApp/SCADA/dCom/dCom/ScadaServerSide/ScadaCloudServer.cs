using Common;
using dCom.Configuration;
using DERMSCommon.DataModel.Core;
using DERMSCommon.SCADACommon;
using Modbus;
using Modbus.Connection;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.ScadaServerSide
{
    public class ScadaCloudServer : SCADACommunication, IScadaCloudServer, Common.IStateUpdater
    {
        public ScadaCloudServer()
        {

        }

        public Dictionary<List<long>, ushort> SendAnalogAndDigitalSignals(Dictionary<long, IdentifiedObject> analogni, Dictionary<long, IdentifiedObject> digitalni)
        {
            configuration = new ConfigReader(analogni, digitalni);
            commandExecutor = new FunctionExecutor(this, configuration);
            scadaManagaer = new ScadaManagaer();
            scadaManagaer.SendGids();

            return GidoviNaAdresu;
        }

        public void LogMessage(string message)
        {

        }

        public void SendCommandToSimlator(ushort length, byte functionCode, ushort outputAddress, ushort value)
        {
            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(length, functionCode, outputAddress, value, configuration);
            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
            commandExecutor.EnqueueCommand(fn);
        }


        public void UpdateConnectionState(Common.ConnectionState currentConnectionState)
        {

        }
    }
}
