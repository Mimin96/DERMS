using CalculationEngineServiceCommon;
using Common;
using DERMSCommon.DataModel.Core;
using Modbus;
using Modbus.FunctionParameters;
using ProcessingModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dCom.Configuration
{
    public class SendListOfGeneratorsToScada : SCADACommunication, ISendListOfGeneratorsToScada
    {
        public void SendListOfGenerators(Dictionary<long, double> generators)
        {
            foreach (KeyValuePair<long, double> generator in generators)
            {

                if (analogniStari.ContainsKey(generator.Key))
                {
                    foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                    {
                        if (generator.Key == gidoviNaAdresu.Key[0] && gidoviNaAdresu.Key[2] == 1)
                        {
                            ushort raw = 0;
                            raw = EGUConverter.ConvertToRaw(2, 5, generator.Value);
                            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, gidoviNaAdresu.Value, raw, configuration);
                            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                            commandExecutor.EnqueueCommand(fn);
                        }
                    }

                }

                else if (digitalniStari.ContainsKey(generator.Key))
                {
                    foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                    {
                        if (generator.Key == gidoviNaAdresu.Key[0] && gidoviNaAdresu.Key[2] == 1)
                        {
                            ushort raw = 0;
                            raw = EGUConverter.ConvertToRaw(2, 5, generator.Value);
                            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, gidoviNaAdresu.Value, raw, configuration);
                            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                            commandExecutor.EnqueueCommand(fn);
                        }
                    }
                }

            }
            // LISTA GENERATORA KOJI SU PROMENILI FLEXIBILITY

        }


    }
}
