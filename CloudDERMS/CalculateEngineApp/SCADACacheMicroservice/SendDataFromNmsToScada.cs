using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        public async Task<bool> SendGids(SignalsTransfer signals)
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
                        using (var tx = _stateManager.CreateTransaction())
                        {
                            var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("AnalogniKontejner").Result;
                            await dictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => value = kvp.Value);
                            await tx.CommitAsync();
                        }

                    }
                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    if (!digitalniStari.ContainsKey(kvp.Key))
                    {
                        digitalniStari.Add(kvp.Key, kvp.Value);
                        using (var tx = _stateManager.CreateTransaction())
                        {
                            var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("DigitalniKontejner").Result;
                            await dictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => value = kvp.Value);
                            await tx.CommitAsync();
                        }

                    }
                }

                string clientAddress = "";

                do
                {
                    using (var tx = _stateManager.CreateTransaction())
                    {
                        IReliableQueue<string> queue = _stateManager.GetOrAddAsync<IReliableQueue<string>>("endpoints").Result;
                        clientAddress = queue.TryPeekAsync(tx).Result.Value;
                    }
                } while (clientAddress == null || clientAddress == "");

                NetTcpBinding binding = new NetTcpBinding();
                var factory = new ChannelFactory<IScadaCloudServer>(binding, new EndpointAddress(clientAddress));
                IScadaCloudServer Proxy = factory.CreateChannel();
                Dictionary<List<long>, ushort> GidoviNaAdresu = Proxy.SendAnalogAndDigitalSignals(analogniStari, digitalniStari);

                using (var tx = _stateManager.CreateTransaction())
                {
                    var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<int, Dictionary<List<long>, ushort>>>("GidoviNaAdresu").Result;
                    await dictionary.AddOrUpdateAsync(tx, 0, GidoviNaAdresu, (key, value) => value = GidoviNaAdresu);
                    await tx.CommitAsync();
                }

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

                            Proxy.SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, par.Value, raw);

                        }
                    }

                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    foreach (KeyValuePair<List<long>, ushort> par in GidoviNaAdresu)
                    {
                        if (par.Key.Contains(((Discrete)kvp.Value).GlobalId))
                        {
                            Proxy.SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, par.Value, (ushort)((Discrete)kvp.Value).NormalValue);
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
