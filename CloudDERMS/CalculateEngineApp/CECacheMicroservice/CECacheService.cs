using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using CalculationEngineServiceCommon;
using DERMSCommon.UIModel.ThreeViewModel;
using CloudCommon;

namespace CECacheMicroservice
{
    public class CECacheService : MessageWriting, ICache
    {
        // fali svuda pubsub notify
        // listOfGeneratorsForScada -- obratiti paznju na ovo kod CEUpdateThroughUI -- metoda Balance 
        // Napravi get metode i update metode za liste za koje je potrebno
        // Proveri na kraju sta se odakle poziva, da li treba da se smesti u interfejs
        #region Reliable Dictionaries
        /*private IReliableDictionary<long, IdentifiedObject> nmsCache; //NetworkModelDictionary
        private IReliableDictionary<long, List<DataPoint>> scadaPointsCached; //SCADAPointsDictionary
        private IReliableDictionary<long, Forecast> derWeatherCached; // DERWeatherCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> productionCached; // ProductionCachedDictionary
        private IReliableDictionary<int, TreeNode<NodeData>> graphCached; // GraphCachedDictionary
        private IReliableDictionary<int, List<DataPoint>> dataPoints; //DataPointsCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> copyOfProductionCached; //CopyOfProductionCachedDictionary
        private Dictionary<long, double> listOfGeneratorsForScada; // ListOfGeneratorsForScadaCachedDictionary
        private IReliableDictionary<int, List<long>> DisableAutomaticOptimization; // DisableAutomaticOptimizationCachedDictionary
        private IReliableDictionary<int, List<long>> TurnedOffGenerators; // TurnedOffGeneratorsCachedDictionary
        private IReliableDictionary<int, List<long>> turnedOnGenerators;  // TurnedOnGeneratorsCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> tempProductionCached; // TempProductionCachedDictionary
        private IReliableDictionary<long, DayAhead> substationDayAhead; //SubstationDayAheadCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> substationsForecast; //SubstationsForecastCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> subGeographicalRegionsForecast; // SubGeographicalRegionsForecastCachedDictionary
        private IReliableDictionary<long, DerForecastDayAhead> generatorForecastList; // GeneratorForecastListCachedDictionary
        private List<NetworkModelTreeClass> networkModelTreeClass;*/
        #endregion

        private TreeNode<NodeData> graph;
        private CloudClient<IPubSub> pubSub;
        private CloudClient<IDERFlexibility> derFlexibility;
        private IReliableStateManager stateManager;
        public CECacheService(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            pubSub = new CloudClient<IPubSub>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEPubSubMicroServiceListener"
            );

            derFlexibility = new CloudClient<IDERFlexibility>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "DERFlexibilityListener"
            );
            //
            graph = null;
            //
        }
        public CECacheService()
        {
            graph = null;
        }

        //public void CalculateNewFlexibility(DataToUI data)
        //public void UpdateMinAndMaxFlexibilityForChangedGenerators()
        //private void CalculateFlexibility()

        #region nmsCache methods
        public async Task PopulateNSMModelCache(NetworkModelTransfer networkModelTransfer)
        {
            MessageReceivedEvent("Information: PopulateNSMModelCache started.");
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Delete)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    //AddNMSModelEntity(io);
                    DeleteNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Insert)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Update)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    //AddNMSModelEntity(io);
                    UpdateNMSModelEntity(io);
                }
            }

            await PopulateGraph(networkModelTransfer);
            await SaveNetworkModelTransfer(networkModelTransfer);
        }
        private async Task SaveNetworkModelTransfer(NetworkModelTransfer networkModelTransfer)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("NetworkModelTransferCachedQueue").Result;
                await dictionary.TryDequeueAsync(tx);
                await dictionary.EnqueueAsync(tx, networkModelTransfer);
                await tx.CommitAsync();
            }
        }
        private NetworkModelTransfer GetNetworkModelTransfer()
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableQueue<NetworkModelTransfer>>("NetworkModelTransferCachedQueue").Result;
                NetworkModelTransfer nmt = dictionary.TryPeekAsync(tx).Result.Value;
                return nmt;
            }
        }
        public async Task<Dictionary<long, IdentifiedObject>> GetNMSModel()
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, IdentifiedObject> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NetworkModelDictionary").Result;

                Dictionary<long, IdentifiedObject> Nmsdictionary = new Dictionary<long, IdentifiedObject>();

                IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        Nmsdictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return Nmsdictionary;
            }
        }
        private Dictionary<long, IdentifiedObject> GetNMSModelLocal()
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, IdentifiedObject> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NetworkModelDictionary").Result;

                Dictionary<long, IdentifiedObject> Nmsdictionary = new Dictionary<long, IdentifiedObject>();

                IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        Nmsdictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return Nmsdictionary;
            }
        }
        public void RestartCache(NetworkModelTransfer networkModelTransfer)
        {
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Delete)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    DeleteNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Insert)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Update)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    UpdateNMSModelEntity(io);
                }
            }
            SaveNetworkModelTransfer(networkModelTransfer).Wait();
        }
        private async void AddNMSModelEntity(IdentifiedObject io)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NetworkModelDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, io.GlobalId, io, (key, value) => value = io);
                await tx.CommitAsync();
            }
        }
        private async void DeleteNMSModelEntity(IdentifiedObject io)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NetworkModelDictionary").Result;
                await dictionary.TryRemoveAsync(tx, io.GlobalId);
                await tx.CommitAsync();
            }
        }
        private async Task UpdateNMSModelEntity(IdentifiedObject io)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NetworkModelDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, io.GlobalId, io, (key, value) => value = io);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region scadaPointsCached methods
        public async Task AddScadaPoints(List<DataPoint> dataPoints)
        {
            List<DataPoint> temp = new List<DataPoint>();
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, List<DataPoint>>>("SCADAPointsDictionary");
                foreach (DataPoint dp in dataPoints)
                {
                    if (!dictionary.Result.ContainsKeyAsync(tx, dp.Gid).Result)
                    {
                        foreach (DataPoint dp1 in dataPoints)
                        {
                            if (dp.Gid.Equals(dp1.Gid))
                            {
                                temp.Add(dp1);
                            }
                        }
                    }
                    await dictionary.Result.AddOrUpdateAsync(tx, dp.Gid, new List<DataPoint>(temp), (key, value) => value = temp);
                    await tx.CommitAsync();
                    temp.Clear();
                }
            }
        }
        public Dictionary<long, List<DataPoint>> GetscadaPointsCached()
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, List<DataPoint>>>("SCADAPointsDictionary").Result;

                Dictionary<long, List<DataPoint>> ScadaPointsCached = new Dictionary<long, List<DataPoint>>();

                IAsyncEnumerable<KeyValuePair<long, List<DataPoint>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, List<DataPoint>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        ScadaPointsCached.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
                return ScadaPointsCached;
            }
        }
        public List<DataPoint> GetScadaDataPoint(long param)
        {
            List<DataPoint> points = new List<DataPoint>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, List<DataPoint>>>("SCADAPointsDictionary").Result;

                points = dict.TryGetValueAsync(tx, param).Result.Value;
            }

            return points;
        }
        public void RemoveFromScadaDataPoint(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SCADAPointsDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        //Ne koristi se nigde 
        //public List<DataPoint> GetDataPoints(long gid)
        //{
        //    if (!scadaPointsCached.ContainsKey(gid))
        //        return null;
        //    return scadaPointsCached[gid];
        //}
        #endregion

        #region derWeatherCached methods
        public async Task PopulateWeatherForecast(NetworkModelTransfer networkModel)
        {
            MessageReceivedEvent("Information: PopulateWeatherForecast started.");
            CloudClient<IDarkSkyApi> transactionCoordinator = new CloudClient<IDarkSkyApi>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEWeatherForecastMicroservice"),
              partitionKey: ServicePartitionKey.Singleton, /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "DarkSkyApiListener"
            );

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key).Wait();
                    }
                    else if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key).Wait();
                    }
                    else if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key).Wait();
                    }

                }
            }
        }
        public async Task AddForecast(Forecast wf, long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, Forecast>>("DERWeatherCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, wf, (key, value) => value = wf);
                await tx.CommitAsync();
            }
        }
        public async Task<Forecast> GetForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, Forecast>>("DERWeatherCachedDictionary");

                if (dictionary.Result.ContainsKeyAsync(tx, gid).Result)
                {
                    Forecast forecast = dictionary.Result.TryGetValueAsync(tx, gid).Result.Value;
                    return forecast;
                }
            }
            return null;
        }
        public async Task PopulateConsumptionForecast(NetworkModelTransfer networkModel)
        {
            MessageReceivedEvent("Information: PopulateConsumptionForecast started.");
            CloudClient<IConsumptionCalculator> transactionCoordinator = new CloudClient<IConsumptionCalculator>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: ServicePartitionKey.Singleton, /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "ConsumptionCalculatorListener"
            );

            Dictionary<long, DerForecastDayAhead> productionCachedDictionary = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DayAhead> substationDayAheadDictionary = new Dictionary<long, DayAhead>();
            Dictionary<long, Forecast> derWeatherCachedDictionary = new Dictionary<long, Forecast>();
            
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        productionCachedDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            //substationDayAhead IReliableDictionary<long, DayAhead> substationDayAhead;
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DayAhead>>("SubstationDayAheadCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        substationDayAheadDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            //derWeatherCached <long, Forecast> derWeatherCached; // DERWeatherCachedDictionary
            derWeatherCachedDictionary = await GetWholeDerWeatherCached();

            //consumptionCalculator.Calculate(productionCached, networkModel,SubstationDayAhead,derWeatherCached);
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.Calculate(productionCachedDictionary, networkModel, substationDayAheadDictionary, derWeatherCachedDictionary)).Wait();
            SendDerForecastDayAhead().Wait();
        }
        //SendDerForecastDayAhead Otkomentarisati metodu 
        public async Task PopulateProductionForecast(NetworkModelTransfer networkModel)
        {
            MessageReceivedEvent("Information: PopulateProductionForecast started.");
            //Communication with Microservice in same application
            CloudClient<IProductionCalculator> transactionCoordinator = new CloudClient<IProductionCalculator>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: ServicePartitionKey.Singleton, /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "ProductionCalculatorListener"
            );

            //<long, DerForecastDayAhead> generatorForecastList; // generatorForecastListCachedDictionary
            Dictionary<long, DerForecastDayAhead> generatorForecastList = await GetGeneratorForecastList();
            //using (var tx = stateManager.CreateTransaction())
            //{
            //    IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;

            //    IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
            //    using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
            //    {
            //        while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //        {
            //            generatorForecastList.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
            //        }
            //    }
            //}

            //private IReliableDictionary<long, DerForecastDayAhead> substationsForecast; SubstationsForecastCachedDictionary
            Dictionary<long, DerForecastDayAhead> substationsForecast = await GetSubstationsForecast();
            //using (var tx = stateManager.CreateTransaction())
            //{
            //    IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;

            //    IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
            //    using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
            //    {
            //        while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //        {
            //            substationsForecast.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
            //        }
            //    }
            //}

            //private IReliableDictionary<long, DerForecastDayAhead> subGeographicalRegionsForecast; // SubGeographicalRegionsForecastCachedDictionary
            Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast = await GetSubGeographicalRegionsForecast();
            //using (var tx = stateManager.CreateTransaction())
            //{
            //    IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;

            //    IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
            //    using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
            //    {
            //        while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //        {
            //            SubGeographicalRegionsForecast.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
            //        }
            //    }
            //}

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateGenerator(GetForecast(kvpDic.Key).Result, gr, generatorForecastList));

                        AddDerForecast(forecastDayAhead, kvpDic.Key, true).Wait(); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
            generatorForecastList = await GetGeneratorForecastList();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateSubstation(GetForecast(kvpDic.Key).Result, gr, networkModel, generatorForecastList, substationsForecast));
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true).Wait();
                        //AddDerForecast(productionCalculator.CalculateSubstation(GetForecast(kvpDic.Key), gr, networkModel, GeneratorForecastList, SubstationsForecast), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }

            substationsForecast = await GetSubstationsForecast();
            SubGeographicalRegionsForecast = await GetSubGeographicalRegionsForecast();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateSubRegion(gr, networkModel, substationsForecast, SubGeographicalRegionsForecast));
                        //AddDerForecast(productionCalculator.CalculateSubRegion(gr, networkModel, SubstationsForecast, SubGeographicalRegionsForecast), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true).Wait();
                    }
                }
            }
            SubGeographicalRegionsForecast = await GetSubGeographicalRegionsForecast();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateGeoRegion(gr, networkModel, SubGeographicalRegionsForecast));
                        //AddDerForecast(productionCalculator.CalculateGeoRegion(gr, networkModel, SubGeographicalRegionsForecast), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true).Wait();
                    }
                }
            }
            
            SendDerForecastDayAhead().Wait();
        }
        public async Task AddDerForecast(DerForecastDayAhead derForecastDayAhead, long gid, bool isInitState)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                var dictionaryCopy = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, derForecastDayAhead, (key, value) => value = derForecastDayAhead);
                await dictionaryCopy.AddOrUpdateAsync(tx, gid, derForecastDayAhead, (key, value) => value = derForecastDayAhead);
                await tx.CommitAsync();
            }

            if (!isInitState)
                SendDerForecastDayAhead().Wait();
        }
        public async Task RemoveFromDerForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        public DerForecastDayAhead GetDerForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary");

                if (!dictionary.Result.ContainsKeyAsync(tx, gid).Result)
                {
                    DerForecastDayAhead forecast = dictionary.Result.TryGetValueAsync(tx, gid).Result.Value;
                    return forecast;
                }
            }
            return null;
        }
        public async Task<Dictionary<long, Forecast>> GetWholeDerWeatherCached()
        {
            Dictionary<long, Forecast> derWeather = new Dictionary<long, Forecast>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, Forecast> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, Forecast>>("DERWeatherCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, Forecast>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, Forecast>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        derWeather.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
               
            }
            return derWeather;
        }



        #endregion

        #region productionCached methods
        //Ne koristi se nigde 
        /*public void PopulateFlexibility(NetworkModelTransfer networkModel)
        {
            DERFlexibility flexibility = new DERFlexibility(networkModel);
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        if (flexibility.CheckFlexibility(gr.GlobalId))
                        {
                            flexibility.TurnOnFlexibility(10, productionCached, gr.GlobalId);
                        }
                    }
                }
            }
        }*/
        public async Task CalculateNewCopyOfProductionCachedFlexibility(Dictionary<long, DerForecastDayAhead> copyOfProductionCachedFlexibility)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead>  copyOfProductionCached = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;

                /*IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> copyOfProductionCachedEnumerable = copyOfProductionCached.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> copyOfProductionCachedEnumerator = copyOfProductionCachedEnumerable.GetAsyncEnumerator())
                {
                    while (copyOfProductionCachedEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        await copyOfProductionCached.AddOrUpdateAsync(tx, copyOfProductionCachedEnumerator.Current.Key, copyOfProductionCachedFlexibility[copyOfProductionCachedEnumerator.Current.Key], (key, value) => value = copyOfProductionCachedFlexibility[copyOfProductionCachedEnumerator.Current.Key]);
                    }
                }
                */

                foreach(DerForecastDayAhead der in copyOfProductionCachedFlexibility.Values)
				{
                    long gid = copyOfProductionCachedFlexibility.Where(x => x.Value.Equals(der)).FirstOrDefault().Key;
                    await copyOfProductionCached.AddOrUpdateAsync(tx,gid,der,(key,value) => value = der);
                }

                await tx.CommitAsync();
            }
        }
        public async Task UpdateProductionCached(Dictionary<long, DerForecastDayAhead> prod)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> ProductionCached = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

                /*IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> copyOfProductionCachedEnumerable = copyOfProductionCached.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> copyOfProductionCachedEnumerator = copyOfProductionCachedEnumerable.GetAsyncEnumerator())
                {
                    while (copyOfProductionCachedEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        await copyOfProductionCached.AddOrUpdateAsync(tx, copyOfProductionCachedEnumerator.Current.Key, copyOfProductionCachedFlexibility[copyOfProductionCachedEnumerator.Current.Key], (key, value) => value = copyOfProductionCachedFlexibility[copyOfProductionCachedEnumerator.Current.Key]);
                    }
                }
                */

                foreach (DerForecastDayAhead der in prod.Values)
                {
                    long gid = prod.Where(x => x.Value.Equals(der)).FirstOrDefault().Key;
                    await ProductionCached.AddOrUpdateAsync(tx, gid, der, (key, value) => value = der);
                }

                await tx.CommitAsync();
            }
        }
        //Poziva se iz metode koja ne bi trebalo da stoji u cache-u/
        public async Task ApplyChangesOnProductionCached(Dictionary<long, double> listOfGeneratorsForScada) // KAD STIGNE POTVRDA SA SKADE DA SU PROMENE IZVRSENE, POZIVAMO OVU METODU KAKO BI NOVI PRORACUNI PROIZVODNJE ZA 24h BILI PRIMENJENI NA CACHE
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> ProductionCachedDictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                IReliableDictionary<long, DerForecastDayAhead> CopyOfProductionCachedDictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;

                await ProductionCachedDictionary.ClearAsync();

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = CopyOfProductionCachedDictionary.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        await ProductionCachedDictionary.AddOrUpdateAsync(tx, dictEnumerator.Current.Key, dictEnumerator.Current.Value, (key, value) => value = dictEnumerator.Current.Value);
                    }
                    await tx.CommitAsync();
                }
            }

            SendDerForecastDayAhead().Wait();
            UpdateMinAndMaxFlexibilityForChangedGenerators(listOfGeneratorsForScada).Wait();
        }
        public async Task SendDerForecastDayAhead()
        {
            pubSub.InvokeWithRetryAsync(client => client.Channel.Notify(CreateDataForUI().Result, (int)Enums.Topics.DerForecastDayAhead)).Wait();
        }
        //NOT COMPLETE CEUpdateThroughUI missing, PubSubCalculatioEngine RETURN
        //public async Task<float> PopulateBalance(long gid)
        //{
        //    CloudClient<ICEUpdateThroughUI> transactionCoordinator = new CloudClient<ICEUpdateThroughUI>
        //    (
        //      serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
        //      partitionKey: new ServicePartitionKey(0), /*CJN*/
        //      clientBinding: WcfUtility.CreateTcpClientBinding(),
        //      listenerName: "CEUpdateThroughUIServiceListener"
        //    );
        //    Dictionary<long, DerForecastDayAhead> productionCachedDictionary = new Dictionary<long, DerForecastDayAhead>();
        //    Dictionary<long, IdentifiedObject> nmsCacheDictionary = new Dictionary<long, IdentifiedObject>();
        //    List<long> turnedOffGeneratorsList = new List<long>();

        //    using (var tx = stateManager.CreateTransaction())
        //    {
        //        IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

        //        IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
        //        using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
        //        {
        //            while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
        //            {
        //                productionCachedDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
        //            }
        //        }
        //    }
        //    using (var tx = stateManager.CreateTransaction())
        //    {
        //        IReliableDictionary<long, IdentifiedObject> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NmsCacheDictionary").Result;

        //        IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
        //        using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
        //        {
        //            while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
        //            {
        //                nmsCacheDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
        //            }
        //        }
        //    }
        //    using (var tx = stateManager.CreateTransaction())
        //    {
        //        IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;

        //        IAsyncEnumerable<KeyValuePair<int, List<long>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
        //        using (IAsyncEnumerator<KeyValuePair<int, List<long>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
        //        {
        //            while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
        //            {
        //                //-->>>PROVERITI DA LI JE OVA LISTA DOBRO POPUNJENA
        //                turnedOffGeneratorsList = dictEnumerator.Current.Value;
        //            }
        //        }
        //    }
        //    float energyFromSource = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.Balance(productionCachedDictionary, gid, nmsCacheDictionary, turnedOffGeneratorsList));
        //    SendDerForecastDayAhead().Wait();

        //    return energyFromSource;
        //    //return 0;
        //}
        //Ne koristi se nigde
        //public DerForecastDayAhead GetDerForecastDayAhead(long gid)
        //{
        //    //if (!productionCached.ContainsKey(gid))
        //    //    return null;
        //    //return productionCached[gid];
        //    return null;
        //}
        public async Task<DataToUI> CreateDataForUI()
        {
            Dictionary<long, DerForecastDayAhead> tempDictionary = new Dictionary<long, DerForecastDayAhead>();
            DataToUI data = new DataToUI();
            tempDictionary = GetAllDerForecastDayAhead().Result;

            data.Data = tempDictionary;
            return data;

            //using (var tx = stateManager.CreateTransaction())
            //{
            //    IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

            //    IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
            //    using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
            //    {
            //        while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //        {
            //            tempDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
            //        }
            //    }
            //    data.Data = tempDictionary;
            //    return data;
            //}
        }
        public async Task<Dictionary<long, DerForecastDayAhead>> GetAllDerForecastDayAhead()
        {
            Dictionary<long, DerForecastDayAhead> tempDictionary = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        tempDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }

                return tempDictionary;
            }
        }
        public async Task AddDerForecastDayAhead(long id, DerForecastDayAhead forecast)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, id, forecast, (key, value) => value = forecast);
                await tx.CommitAsync();
            }

        }
        public async Task RemoveFromDerForecastDayAhead(long id)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }

        }
        public async Task<Dictionary<long, DerForecastDayAhead>> GetDerForecasts()
        {
            Dictionary<long, DerForecastDayAhead> productionCachedDictionary = new Dictionary<long, DerForecastDayAhead>();
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        productionCachedDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }
            return productionCachedDictionary;
        }
        #endregion

        #region TreeGraph Methods
        //NOT COMPLETE CALC FLEXIBILITY treba da se pozove odavde //CalculateFlexibility -- calc flexibility bi trebalo da stoji u nekom drugom servisu
        private async Task PopulateGraph(NetworkModelTransfer networkModelTransfer)
        {
            MessageReceivedEvent("Information: PopulateGraph started.");
            CloudClient<ITreeConstruction> transactionCoordinator = new CloudClient<ITreeConstruction>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/TreeConstructionMicroservice"),
                partitionKey: ServicePartitionKey.Singleton,
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "BuildTreeServiceListener"
            );

            graph = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.ConstructTree1(networkModelTransfer)).Result;

            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, 0, graph, (key, value) => value = graph);
                await tx.CommitAsync();
            }


            //using (var tx = stateManager.CreateTransaction())
            //{//GraphCachedDictionary
            //    var dictionary = stateManager.GetOrAddAsync<IReliableQueue<TreeNode<NodeData>>>("GraphCachedQueue").Result;
            //    await dictionary.TryDequeueAsync(tx);
            //    await dictionary.EnqueueAsync(tx,graph);
            //   // await dictionary.AddOrUpdateAsync(tx, 0, graph, (key, value) => value = graph);
            //    await tx.CommitAsync();
            //}

            // ovaj deo koda nema smisla jer je graf vec tu iznad
            //List<NetworkModelTreeClass >nn = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNetworkModelTreeClass());
        }
        public async Task UpdateGraphWithScadaValues(List<DataPoint> data)
        {
            CloudClient<ITreeConstruction> transactionCoordinator = new CloudClient<ITreeConstruction>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/TreeConstructionMicroservice"),
                partitionKey: ServicePartitionKey.Singleton, /*CJN*/
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "BuildTreeServiceListener"
            );

            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result;
                //Upitno da li radi ovaj upit
                //TreeNode<NodeData> graphToSend = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result.TryGetValueAsync(tx, 0).Result.Value;
                TreeNode<NodeData> graphToSend = GetGraph().Result;
                TreeNode<NodeData> graph = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.UpdateGraphWithScadaValues(data, graphToSend)).Result;
                await dictionary.AddOrUpdateAsync(tx, 0, graph, (key, value) => value = graph);
                await tx.CommitAsync();
            }
        }
        public async Task <TreeNode<NodeData>> GetGraph()
        {
            CloudClient<ITreeConstruction> transactionCoordinator = new CloudClient<ITreeConstruction>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/TreeConstructionMicroservice"),
                partitionKey: ServicePartitionKey.Singleton,
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "BuildTreeServiceListener"
            );

            TreeNode<NodeData> graph = new TreeNode<NodeData>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, TreeNode<NodeData>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result;

                graph = dict.TryGetValueAsync(tx, 0).Result.Value;
            }

            return graph;
        }
        #endregion

        #region datapoints Methods
        //HAS TO BE CHECKED
        public async Task UpdateNewDataPoitns(List<DataPoint> points)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                List<DataPoint> dataPoints = GetDatapoints().Result;
                if (dataPoints == null)
                {
                    dataPoints = new List<DataPoint>();
                }

                foreach (DataPoint data in points)
                {
                    if (dataPoints.Where(x => x.Gid == data.Gid).Count() == 0)
                    {
                        dataPoints.Add(data);
                        await AddToDataPoints(data);
                    }
                    else
                    {
                        dataPoints[dataPoints.FindIndex(ind => ind.Gid == data.Gid)] = data;
                        await AddToDataPoints(data);
                    }
                }
                
                await tx.CommitAsync();
            }
        }
        public async Task<List<DataPoint>> GetDatapoints()
        {
            List<DataPoint> dPoints = new List<DataPoint>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;

                dPoints = dict.TryGetValueAsync(tx, 0).Result.Value;
            }

            return dPoints;
        }
        public async Task AddToDataPoints(DataPoint datapoint)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;
                List<DataPoint> points = dict.TryGetValueAsync(tx, 0).Result.Value;
                if (points == null)
                    points = new List<DataPoint>();

                if (points.Where(x => x.Gid == datapoint.Gid).FirstOrDefault() != null)
                {
                    DataPoint data = points.Where(x => x.Gid == datapoint.Gid).FirstOrDefault();
                    points.Remove(data);
                }
                points.Add(datapoint);

                await dict.AddOrUpdateAsync(tx, 0, points, (key, value) => value = points);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromDataPoints(DataPoint datapoint)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;
                List<DataPoint> points = dict.TryGetValueAsync(tx, 0).Result.Value;
                points.Remove(datapoint);

                await dict.AddOrUpdateAsync(tx, 0, points, (key, value) => value = points);
            }
        }
        #endregion

        #region copyOfProductionCached methods
        //ApplyChangesOnProductionCached
        //CalculateNewFlexibility -- vec postoje 
        public async Task<Dictionary<long, DerForecastDayAhead>> GetCopyOfProductionCached()
        {
            Dictionary<long, DerForecastDayAhead> CopyOfProductionCached = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        CopyOfProductionCached.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return CopyOfProductionCached;
        }
        public async Task AddToCopyOfProductionCached(long gid, DerForecastDayAhead forecast)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, forecast, (key, value) => value = forecast);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromCopyOfProductionCached(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region listOfGeneratorsForScada methods
        //CalculateNewFlexibility
        //NOT COMPLETE -- Trebalo bi da stoje na nekom drugom servisu
        public async Task UpdateMinAndMaxFlexibilityForChangedGenerators(Dictionary<long, double> listOfGeneratorsForScada)
        {
            MessageReceivedEvent("Information: UpdateMinAndMaxFlexibilityForChangedGenerators started.");
            double minProd = 0;
            double maxProd = 0;
            double currentProd = 0;

            List<NetworkModelTreeClass> networkModelTreeClass = GetNetworkModelTreeClass().Result;
            Dictionary<long, IdentifiedObject> nmsCache = GetNMSModel().Result;

            foreach (NetworkModelTreeClass networkModelTreeClasses in networkModelTreeClass)
            {
                foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
                {
                    foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
                    {
                        foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
                        {
                            foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
                            {
                                if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
                                {
                                    if (listOfGeneratorsForScada.ContainsKey(substationElementTreeClass.GID))
                                    {
                                        IdentifiedObject gen = nmsCache[substationElementTreeClass.GID];

                                        maxProd = substationElementTreeClass.P + substationElementTreeClass.P * (substationElementTreeClass.MaxFlexibility / 100);
                                        minProd = substationElementTreeClass.P - substationElementTreeClass.P * (substationElementTreeClass.MinFlexibility / 100);

                                        currentProd = substationElementTreeClass.P + substationElementTreeClass.P * (listOfGeneratorsForScada[substationElementTreeClass.GID] / 100);

                                        substationElementTreeClass.P = (float)currentProd;
                                        substationElementTreeClass.MaxFlexibility = (float)(((maxProd - currentProd) * 100) / currentProd);
                                        substationElementTreeClass.MinFlexibility = (float)(((currentProd - minProd) * 100) / currentProd);

                                        ((Generator)gen).ConsiderP = substationElementTreeClass.P;
                                        ((Generator)gen).MaxFlexibility = substationElementTreeClass.MaxFlexibility;
                                        ((Generator)gen).MinFlexibility = substationElementTreeClass.MinFlexibility;
                                        UpdateNMSModelEntity(gen).Wait();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            SetNetworkModelTreeClass(networkModelTreeClass).Wait();
            CalculateFlexibility(networkModelTreeClass).Wait();            
        }
        //NOT COMPLETE -- Trebalo bi da stoje na nekom drugom servisu
        public async Task CalculateFlexibility(List<NetworkModelTreeClass> NetworkModelTreeClass)
        {
            MessageReceivedEvent("Information: CalculateFlexibility started.");
            float minFlexibilitySubstation = 0;
            float maxFlexibilitySubstation = 0;
            float productionSubstation = 0;

            float minFlexibilitySubRegion = 0;
            float maxFlexibilitySubRegion = 0;
            float productionSubRegion = 0;

            float minFlexibilityGeoRegion = 0;
            float maxFlexibilityGeoRegion = 0;
            float productionGeoRegion = 0;

            float minFlexibilityNetworkModel = 0;
            float maxFlexibilityNetworkModel = 0;
            float productionNetworkModel = 0;

            List<NetworkModelTreeClass> networkModelTreeClass = await GetNetworkModelTreeClass();
            //if (networkModelTreeClass == null)
            networkModelTreeClass = NetworkModelTreeClass; 

            foreach (NetworkModelTreeClass networkModelTreeClasses in networkModelTreeClass)
            {
                foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
                {
                    foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
                    {
                        foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
                        {
                            foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
                            {
                                if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
                                {
                                    productionSubstation += substationElementTreeClass.P;
                                    minFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MinFlexibility) / 100;
                                    maxFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MaxFlexibility) / 100;
                                }
                            }

                            substationTreeClass.MinFlexibility = (100 * minFlexibilitySubstation) / productionSubstation;
                            substationTreeClass.MaxFlexibility = (100 * maxFlexibilitySubstation) / productionSubstation;

                            productionSubRegion += productionSubstation;
                            minFlexibilitySubRegion += (productionSubstation * substationTreeClass.MinFlexibility) / 100;
                            maxFlexibilitySubRegion += (productionSubstation * substationTreeClass.MaxFlexibility) / 100;

                            minFlexibilitySubstation = 0;
                            maxFlexibilitySubstation = 0;
                            productionSubstation = 0;
                        }

                        geographicalSubRegionTreeClass.MinFlexibility = (100 * minFlexibilitySubRegion) / productionSubRegion;
                        geographicalSubRegionTreeClass.MaxFlexibility = (100 * maxFlexibilitySubRegion) / productionSubRegion;

                        productionGeoRegion += productionSubRegion;
                        minFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MinFlexibility) / 100;
                        maxFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MaxFlexibility) / 100;

                        minFlexibilitySubRegion = 0;
                        maxFlexibilitySubRegion = 0;
                        productionSubRegion = 0;
                    }

                    geographicalRegionTreeClass.MinFlexibility = (100 * minFlexibilityGeoRegion) / productionGeoRegion;
                    geographicalRegionTreeClass.MaxFlexibility = (100 * maxFlexibilityGeoRegion) / productionGeoRegion;

                    productionNetworkModel += productionGeoRegion;
                    minFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MinFlexibility) / 100;
                    maxFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MaxFlexibility) / 100;

                    minFlexibilityGeoRegion = 0;
                    maxFlexibilityGeoRegion = 0;
                    productionGeoRegion = 0;

                }

                networkModelTreeClasses.MinFlexibility = (100 * minFlexibilityNetworkModel) / productionNetworkModel;
                networkModelTreeClasses.MaxFlexibility = (100 * maxFlexibilityNetworkModel) / productionNetworkModel;

                minFlexibilityNetworkModel = 0;
                maxFlexibilityNetworkModel = 0;
                productionNetworkModel = 0;
            }

            DataToUI data = new DataToUI();
            data.NetworkModelTreeClass = networkModelTreeClass;
            SetNetworkModelTreeClass(networkModelTreeClass).Wait();
            pubSub.InvokeWithRetryAsync(client => client.Channel.Notify(data, (int)Enums.Topics.NetworkModelTreeClass)).Wait();
        }
        public Dictionary<long, double> GetListOfGeneratorsForScada()
        {
            Dictionary<long, double> ListOfGeneratorsForScada = new Dictionary<long, double>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, double> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, double>>("ListOfGeneratorsForScadaCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, double>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, double>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        ListOfGeneratorsForScada.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return ListOfGeneratorsForScada;
        }
        public async Task AddToListOfGeneratorsForScada(long gid, double param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, double>>("ListOfGeneratorsForScadaCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromListOfGeneratorsForScada(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, double>>("ListOfGeneratorsForScadaCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region DisableAutomaticOptimization methods
        //CHECK DA LI TREBA DA SE UBACE U interfejs
        //NOT COMPLETE 
        //ListOfDisabledGenerators
        //AllowOptimization
        //ListOffTurnedOffGenerators -- Stoje negde drugde ali traze vrednosti te liste, ali ne stoje u cache
        //This method should not be used
        public async Task<Dictionary<int, List<long>>> GetDisableAutomaticOptimization() //DisableAutomaticOptimizationCachedDictionary
        {
            Dictionary<int, List<long>> DisableAutomaticOptimization = new Dictionary<int, List<long>>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<int, List<long>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<int, List<long>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        DisableAutomaticOptimization.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return DisableAutomaticOptimization;
        }
        public async Task<List<long>> GetDisableAutomaticOptimizationList()
        {
            List<long> lista = new List<long>();
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;

                lista = dict.TryGetValueAsync(tx, 0).Result.Value;
            }
            return lista;
        }
        public async Task AddToDisableAutomaticOptimization(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                if (lista == null)
                    lista = new List<long>();
                lista.Add(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromDisableAutomaticOptimization(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Remove(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region TurnedOffGenerators methods
        //Balance 
        //GeneratorOffCheck
        //ChangeBreakerStatus -- Stoje negde drugde ali traze vrednosti liste, ali ove metode ne stoje u cache (FlexibilityFromUIToCE, CEUpdateThroughUI) 
        //TurnedOffGeneratorsCachedDictionary
        public async Task<Dictionary<int, List<long>>> GetTurnedOffGenerators()
        {
            Dictionary<int, List<long>> TurnedOffGenerators = new Dictionary<int, List<long>>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<long>> dict =  stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<int, List<long>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<int, List<long>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        TurnedOffGenerators.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return TurnedOffGenerators;
        }
        public async Task AddToTurnedOffGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                if (lista == null)
                    lista = new List<long>();
                lista.Add(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromTurnedOffGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Remove(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region turnedOnGenerators methods
        //ListOffTurnedOffGenerators 
        //ChangeBreakerStatus -- Stoje negde drugde ali traze vrednosti liste, ali ove metode ne stoje u cache (FlexibilityFromUIToCE, CEUpdateThroughUI) 
        public async Task<Dictionary<int, List<long>>> GetTurnedOnGenerators()
        {
            Dictionary<int, List<long>> TurnedOnGenerators = new Dictionary<int, List<long>>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOnGeneratorsCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<int, List<long>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<int, List<long>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        TurnedOnGenerators.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return TurnedOnGenerators;
        }
        public async Task AddToTurnedOnGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOnGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                if (lista == null)
                    lista = new List<long>();
                lista.Add(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromTurnedOnGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOnGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                if (lista == null)
                    lista = new List<long>();
                lista.Remove(param);
                await dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region tempProductionCached methods
        //ChangeBreakerStatus
        public async Task<Dictionary<long, DerForecastDayAhead>> GetTempProductionCached()
        {
            Dictionary<long, DerForecastDayAhead> TempProductionCached = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("TempProductionCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        TempProductionCached.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return TempProductionCached;
        }
        public async Task AddToTempProductionCached(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("TempProductionCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromTempProductionCached(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("TempProductionCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region substationDayAhead methods
        //PopulateConsumptionForecast GORE U KODU, tamo treba implementirati
        public Dictionary<long, DayAhead> GetSubstationDayAhead() //SubstationDayAheadCachedDictionary
        {
            Dictionary<long, DayAhead> SubstationDayAhead = new Dictionary<long, DayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DayAhead>>("SubstationDayAheadCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        SubstationDayAhead.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return SubstationDayAhead;
        }
        public async Task AddToSubstationDayAhead(long gid, DayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DayAhead>>("SubstationDayAheadCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public void RemoveFromSubstationDayAhead(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DayAhead>>("SubstationDayAheadCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

        #region substationsForecast methods
        //PopulateProductionForecast GORE U KODU, tamo treba implementirati
        public async Task<Dictionary<long, DerForecastDayAhead>> GetSubstationsForecast() //SubstationsForecastCachedDictionary
        {
            Dictionary<long, DerForecastDayAhead> SubstationsForecast = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        SubstationsForecast.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return SubstationsForecast;
        }
        public async Task AddToSubstationsForecast(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public void RemoveFromSubstationsForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

        #region subGeographicalRegionsForecast methods
        //PopulateProductionForecast GORE U KODU, tamo treba implementirati
        public async Task<Dictionary<long, DerForecastDayAhead>> GetSubGeographicalRegionsForecast() // SubGeographicalRegionsForecastCachedDictionary
        {
            Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        SubGeographicalRegionsForecast.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return SubGeographicalRegionsForecast;
        }
        public async Task AddToSubGeographicalRegionsForecast(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromSubGeographicalRegionsForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region generatorForecastList methods
        //PopulateProductionForecast GORE U KODU, tamo treba implementirati
        public async Task<Dictionary<long, DerForecastDayAhead>> GetGeneratorForecastList() // GeneratorForecastListCachedDictionary
        {
            Dictionary<long, DerForecastDayAhead> GeneratorForecastList = new Dictionary<long, DerForecastDayAhead>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        GeneratorForecastList.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            return GeneratorForecastList;

        }
        public async Task AddToGeneratorForecastList(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                await tx.CommitAsync();
            }
        }
        public async Task RemoveFromGeneratorForecastList(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;
                await dictionary.TryRemoveAsync(tx, gid);
                await tx.CommitAsync();
            }
        }
        #endregion

        #region networkModelTreeClass methods
        // get nmt 
        public async Task SetNetworkModelTreeClass(List<NetworkModelTreeClass> networkModelTreeClass)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<NetworkModelTreeClass>>>("NetworkModelTreeClassCached").Result;
                await dictionary.AddOrUpdateAsync(tx, 0, networkModelTreeClass, (key, value) => value = networkModelTreeClass);
                await tx.CommitAsync();
            }
        }
        public async Task<List<NetworkModelTreeClass>> GetNetworkModelTreeClass()
        {
            List<NetworkModelTreeClass> nmt = new List<NetworkModelTreeClass>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<NetworkModelTreeClass>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<NetworkModelTreeClass>>>("NetworkModelTreeClassCached").Result;

                nmt = dict.TryGetValueAsync(tx, 0).Result.Value;
            }

            return nmt;
        }

        #endregion

    }
}
