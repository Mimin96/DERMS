using CalculationEngineServiceCommon;
using Common;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
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

                //if (analogniStari.ContainsKey(generator.Key))
                //{
                foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                {
                    if (generator.Key == gidoviNaAdresu.Key[0])
                    {
                        if (analogniStari.Keys.Contains(gidoviNaAdresu.Key[1]))
                        {
                            if (analogniStari[gidoviNaAdresu.Key[1]].Description == "Commanding")
                            {
                                {

                                    KeyValuePair<long,IdentifiedObject> a = analogniStari.ElementAt(gidoviNaAdresu.Value - 3000-1);
                                    float zbir = ((Analog)a.Value).NormalValue + (float)generator.Value * ((Analog)a.Value).NormalValue/100;
                                    ((Analog)a.Value).NormalValue = zbir;
                                    zbir = (float)Math.Round(zbir);
                                    double vred = (generator.Value * ((Analog)a.Value).NormalValue / 100);
                                    vred = (double)Math.Round(vred);
                                    if (vred < 0)
                                    {
                                        vred = vred * (-1); 
                                    }

                                    ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, gidoviNaAdresu.Value, (ushort)vred, configuration);
                                    Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                                    commandExecutor.EnqueueCommand(fn);
                                    ModbusWriteCommandParameters p1 = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(gidoviNaAdresu.Value-2), (ushort)zbir, configuration);
                                    Common.IModbusFunction fn1 = FunctionFactory.CreateModbusFunction(p1);
                                    commandExecutor.EnqueueCommand(fn1);
                                }
                            }
                        }
                        else if (digitalniStari.Keys.Contains(gidoviNaAdresu.Key[1]))
                        {
                            if (digitalniStari[gidoviNaAdresu.Key[1]].Description == "Commanding")
                            {
                                {

                                    ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, gidoviNaAdresu.Value, (ushort)generator.Value, configuration);
                                    Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                                    commandExecutor.EnqueueCommand(fn);
                                }
                            }
                        }
                    }


                    //else if (digitalniStari.ContainsKey(generator.Key))
                    //{
                    //    foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                    //    {
                    //        if (generator.Key == gidoviNaAdresu.Key[0] && gidoviNaAdresu.Key[2] == 1)
                    //        {
                    //            ushort raw = 0;
                    //            raw = EGUConverter.ConvertToRaw(2, 5, generator.Value);
                    //            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, gidoviNaAdresu.Value, raw, configuration);
                    //            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                    //            commandExecutor.EnqueueCommand(fn);
                    //        }
                    //    }
                    //}

                }
                // LISTA GENERATORA KOJI SU PROMENILI FLEXIBILITY

            }


        }
    }
}

