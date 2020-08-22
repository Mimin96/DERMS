using DERMSCommon.DataModel.Core;
using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADACacheMicroservice
{
     public class CloudScadaToLocalScada : IScadaCloudToScadaLocal
    {
        private IReliableStateManager _stateManager;

        public CloudScadaToLocalScada(IReliableStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task <bool> SendEndpoints(string endpoint)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<string> queue = _stateManager.GetOrAddAsync<IReliableQueue<string>>("endpoints").Result;
                await queue.EnqueueAsync(tx, endpoint);
                await tx.CommitAsync();
            }
            return true;
        }

        public async Task AddorUpdateAnalogniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict)
        {
            foreach (KeyValuePair<long, IdentifiedObject> kvp in dict)
            {
                using (var tx = _stateManager.CreateTransaction())
                {
                    var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("AnalogniKontejner").Result;
                    await dictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => value = kvp.Value);
                    await tx.CommitAsync();
                }
            }
        }

        public async Task<string> GetAddress()
        {
            string returnVal = "";
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableQueue<string> queue = _stateManager.GetOrAddAsync<IReliableQueue<string>>("endpoints").Result;
                 returnVal = queue.TryPeekAsync(tx).Result.Value;
            }
            return returnVal;
        }

        public async Task AddorUpdateDigitalniKontejnerModelEntity(Dictionary<long, IdentifiedObject> dict)
        {
            foreach (KeyValuePair<long, IdentifiedObject> kvp in dict)
            {
                using (var tx = _stateManager.CreateTransaction())
                {
                    var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("DigitalniKontejner").Result;
                    await dictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => value = kvp.Value);
                    await tx.CommitAsync();
                }
            }
        }

        public async Task AddorUpdateGidoviNaAdresuModelEntity(Dictionary<List<long>, ushort> dict)
        {
                using (var tx = _stateManager.CreateTransaction())
                {
                    var dictionary = _stateManager.GetOrAddAsync<IReliableDictionary<int, Dictionary<List<long>, ushort>>>("GidoviNaAdresu").Result;
                    await dictionary.AddOrUpdateAsync(tx, 0, dict, (key, value) => value = dict);
                    await tx.CommitAsync();
                }
        }

        public async Task<Dictionary<long, IdentifiedObject>> GetAnalogniKontejnerModel()
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableDictionary<long, IdentifiedObject> dict = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("AnalogniKontejner").Result;

                Dictionary<long, IdentifiedObject> AnalogniDict = new Dictionary<long, IdentifiedObject>();

                IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        AnalogniDict.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return AnalogniDict;
            }
        }

        public async Task<Dictionary<long, IdentifiedObject>> GetDigitalniKontejnerModel()
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableDictionary<long, IdentifiedObject> dict = _stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("DigitalniKontejner").Result;

                Dictionary<long, IdentifiedObject> DigitalniDict = new Dictionary<long, IdentifiedObject>();

                IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        DigitalniDict.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return DigitalniDict;
            }
        }

        public async Task<Dictionary<List<long>, ushort>> GetGidoviNaAdresuModel()
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                IReliableDictionary<int, Dictionary<List<long>, ushort>> dict = _stateManager.GetOrAddAsync<IReliableDictionary<int, Dictionary<List<long>, ushort>>>("GidoviNaAdresu").Result;

                Dictionary< int, Dictionary<List<long>, ushort> > GidoviDict = new Dictionary<int, Dictionary<List<long>, ushort>>();

                IAsyncEnumerable<KeyValuePair<int, Dictionary<List<long>, ushort>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<int, Dictionary<List<long>, ushort>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        GidoviDict.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return GidoviDict[0];
            }
        }
    }
}
