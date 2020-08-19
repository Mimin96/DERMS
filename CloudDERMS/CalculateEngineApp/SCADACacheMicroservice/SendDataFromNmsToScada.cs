using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.NMSCommuication;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACacheMicroservice
{
    public class SendDataFromNmsToScada : ISendDataFromNMSToScada
    {
        private IReliableStateManager _stateManager;

        public SendDataFromNmsToScada(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task<bool> CheckForTM(SignalsTransfer signals)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<SignalsTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<SignalsTransfer>>("signalsTransfer").Result;

                SignalsTransfer modelTransfer = queue.TryDequeueAsync(tx).Result.Value;
                await queue.EnqueueAsync(tx, signals);

                await tx.CommitAsync();
            }

            if (signals != null)
                return true;
            else
                return false;
        }

        public Task<bool> SendGids(SignalsTransfer signals)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<SignalsTransfer> queue = _stateManager.GetOrAddAsync<IReliableQueue<SignalsTransfer>>("networkModelTransfer").Result;

                signals = queue.TryPeekAsync(tx).Result.Value;
            }

            Dictionary<long, IdentifiedObject> analogni = new Dictionary<long, IdentifiedObject>();
            Dictionary<long, IdentifiedObject> digitalni = new Dictionary<long, IdentifiedObject>();

           Dictionary<long, IdentifiedObject> digitalniStari = new Dictionary<long, IdentifiedObject>();
           Dictionary<long, IdentifiedObject> analogniStari = new Dictionary<long, IdentifiedObject>();

            if (signals.Insert.Count > 0 || signals.Update.Count > 0)
            {

                analogni = signals.Insert[1];
                digitalni = signals.Insert[0];
                foreach (KeyValuePair<long, IdentifiedObject> kvp in analogni)
                {
                    if (!analogniStari.ContainsKey(kvp.Key))
                    {
                        analogniStari.Add(kvp.Key, kvp.Value);

                    }
                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    if (!digitalniStari.ContainsKey(kvp.Key))
                    {
                        digitalniStari.Add(kvp.Key, kvp.Value);

                    }
                }

                configuration = new ConfigReader(analogniStari, digitalniStari);

                foreach (KeyValuePair<long, IdentifiedObject> kvp in analogni)
                {

                    foreach (KeyValuePair<List<long>, ushort> par in GidoviNaAdresu)
                    {
                        if (par.Key.Contains(((Analog)kvp.Value).GlobalId))
                        {
                            ushort raw = 0;
                            if ((double)((Analog)kvp.Value).NormalValue != 0)
                            {
                                raw = (ushort)((Analog)kvp.Value).NormalValue;
                            }

                            //SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, par.Value, raw, configuration);

                        }
                    }

                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    foreach (KeyValuePair<List<long>, ushort> par in GidoviNaAdresu)
                    {
                        if (par.Key.Contains(((Discrete)kvp.Value).GlobalId))
                        {
                            // sa clouda lokal
                            //SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, par.Value, (ushort)((Discrete)kvp.Value).NormalValue, configuration);
                        }
                    }

                }

        
            }

            if (signals != null)
                return true;
            else
                return false;
        }
    }
}
