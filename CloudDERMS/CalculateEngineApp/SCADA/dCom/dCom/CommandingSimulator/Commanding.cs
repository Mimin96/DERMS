using Common;
using Modbus;
using Modbus.Connection;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.CommandingSimulator
{
    public class Commanding : IStateUpdater
    {
        FunctionExecutor commandExecutor;

        public Commanding()
        {

        }

        public void LogMessage(string message)
        {
            
        }

        public void SendCommandToSimlator(ushort length, byte functionCode, ushort outputAddress, ushort value, global::Common.IConfiguration configuration)
        {
            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(length, functionCode, outputAddress, value, configuration);
            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
            commandExecutor = new FunctionExecutor(this, configuration);
            commandExecutor.EnqueueCommand(fn);
        }

        public void UpdateConnectionState(ConnectionState currentConnectionState)
        {
            
        }
    }
}
