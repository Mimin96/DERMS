﻿using CloudCommon.CalculateEngine;
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

namespace CECacheMicroservice
{
    public class CECacheService : ICache
    {
        // fali svuda pubsub notify
        // listOfGeneratorsForScada -- obratiti paznju na ovo kod CEUpdateThroughUI -- metoda Balance 
        // Napravi get metode i update metode za liste za koje je potrebno
        // Proveri na kraju sta se odakle poziva, da li treba da se smesti u interfejs
        #region Reliable Dictionaries
        //private IReliableDictionary<long, IdentifiedObject> nmsCache; //NetworkModelDictionary
        //private IReliableDictionary<long, List<DataPoint>> scadaPointsCached; //SCADAPointsDictionary
        //private IReliableDictionary<long, Forecast> derWeatherCached; // DERWeatherCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> productionCached; // ProductionCachedDictionary
        //private IReliableDictionary<int, TreeNode<NodeData>> graphCached; // GraphCachedDictionary
        //private IReliableDictionary<int, List<DataPoint>> dataPoints; //DataPointsCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> copyOfProductionCached; //CopyOfProductionCachedDictionary
        //private IReliableDictionary<long, double> listOfGeneratorsForScada; // ListOfGeneratorsForScadaCachedDictionary
        //private IReliableDictionary<int, List<long>> DisableAutomaticOptimization; // DisableAutomaticOptimizationCachedDictionary
        //private IReliableDictionary<int, List<long>> TurnedOffGenerators; // TurnedOffGeneratorsCachedDictionary
        //private IReliableDictionary<int, List<long>> turnedOnGenerators;  // TurnedOnGeneratorsCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> tempProductionCached; // TempProductionCachedDictionary
        //private IReliableDictionary<long, DayAhead> substationDayAhead; //SubstationDayAheadCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> substationsForecast; //SubstationsForecastCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> subGeographicalRegionsForecast; // SubGeographicalRegionsForecastCachedDictionary
        //private IReliableDictionary<long, DerForecastDayAhead> generatorForecastList; // GeneratorForecastListCachedDictionary
        //private IReliableDictionary<int, List<NetworkModelTreeClass>> networkModelTreeClass;
        #endregion

        private CloudClient<IPubSub> pubSub;
        private CloudClient<IDERFlexibility> derFlexibility;
        private IReliableStateManager stateManager;
        public CECacheService(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            pubSub = new CloudClient<IPubSub>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroService"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEPubSubMicroServiceListener"
            );

            derFlexibility = new CloudClient<IDERFlexibility>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "DERFlexibilityListener"
            );
        }
        public CECacheService()
        {
           
        }

        //public void CalculateNewFlexibility(DataToUI data)
        //public void UpdateMinAndMaxFlexibilityForChangedGenerators()
        //private void CalculateFlexibility()

        #region nmsCache methods
        public async Task PopulateNSMModelCache(NetworkModelTransfer networkModelTransfer)
        {
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

            PopulateGraph(networkModelTransfer);
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
        private async void UpdateNMSModelEntity(IdentifiedObject io)
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
        public void AddScadaPoints(List<DataPoint> dataPoints)
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
                    dictionary.Result.AddOrUpdateAsync(tx, dp.Gid, new List<DataPoint>(temp), (key, value) => value = temp);
                    tx.CommitAsync();
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
            //Communication with Microservice in same application
            //ServicePartitionKey(0)
            CloudClient<IDarkSkyApi> transactionCoordinator = new CloudClient<IDarkSkyApi>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CEWeatherForecastMicroservice"),
              partitionKey:  ServicePartitionKey.Singleton,
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
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key);
                    }
                    else if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key);
                    }
                    else if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
                        AddForecast(await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetWeatherForecastAsync(gr.Latitude, gr.Longitude)), kvpDic.Key);
                    }

                }
            }
        }
        public async void AddForecast(Forecast wf, long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, Forecast>>("DERWeatherCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, gid, wf, (key, value) => value = wf);
                await tx.CommitAsync();
            }
        }
        public Forecast GetForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, List<Forecast>>>("DERWeatherCachedDictionary");

                if (!dictionary.Result.ContainsKeyAsync(tx, gid).Result)
                {
                    Forecast forecast = dictionary.Result.TryGetValueAsync(tx, gid).Result.Value.ToList().First();
                    return forecast;
                }
            }
            return null;
        }
        //NOT COMPLETE PubSubCalculatioEngine
        public async Task PopulateConsumptionForecast(NetworkModelTransfer networkModel)
        {
            //Communication with Microservice in same application
            CloudClient<IConsumptionCalculator> transactionCoordinator = new CloudClient<IConsumptionCalculator>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "ConsumptionCalculatorListener"
            );

            Dictionary<long, DerForecastDayAhead> productionCachedDictionary = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DayAhead> substationDayAheadDictionary = new Dictionary<long, DayAhead>();
            Dictionary<long, Forecast> derWeatherCachedDictionary = new Dictionary<long, Forecast>();

            //productionCached
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
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, Forecast> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, Forecast>>("DERWeatherCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, Forecast>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, Forecast>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        derWeatherCachedDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.Calculate(productionCachedDictionary, networkModel, substationDayAheadDictionary, derWeatherCachedDictionary));

            //PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
        }
        //NOT COMPLETE PubSubCalculatioEngine
        public async Task PopulateProductionForecast(NetworkModelTransfer networkModel)
        {
            //Communication with Microservice in same application
            CloudClient<IProductionCalculator> transactionCoordinator = new CloudClient<IProductionCalculator>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "ProductionCalculatorListener"
            );

            //<long, DerForecastDayAhead> generatorForecastList; // generatorForecastListCachedDictionary
            Dictionary<long, DerForecastDayAhead> generatorForecastList = new Dictionary<long, DerForecastDayAhead>();
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        generatorForecastList.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            //private IReliableDictionary<long, DerForecastDayAhead> substationsForecast; SubstationsForecastCachedDictionary
            Dictionary<long, DerForecastDayAhead> substationsForecast = new Dictionary<long, DerForecastDayAhead>();
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        substationsForecast.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }

            //private IReliableDictionary<long, DerForecastDayAhead> subGeographicalRegionsForecast; // SubGeographicalRegionsForecastCachedDictionary
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

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateGenerator(GetForecast(kvpDic.Key), gr, generatorForecastList));

                        AddDerForecast(forecastDayAhead, kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        DerForecastDayAhead forecastDayAhead = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.CalculateSubstation(GetForecast(kvpDic.Key), gr, networkModel, generatorForecastList, substationsForecast));
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true);
                        //AddDerForecast(productionCalculator.CalculateSubstation(GetForecast(kvpDic.Key), gr, networkModel, GeneratorForecastList, SubstationsForecast), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
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
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true);
                    }
                }
            }
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
                        AddDerForecast(forecastDayAhead, kvpDic.Key, true);
                    }
                }
            }



            //PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead); // KAD SE POPUNI CACHE SALJE SVIMA Dictionary
        }
        //NOT COMPLETE PubSubCalculatioEngine
        public void AddDerForecast(DerForecastDayAhead derForecastDayAhead, long gid, bool isInitState)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, derForecastDayAhead, (key, value) => value = derForecastDayAhead);
                tx.CommitAsync();
            }

            //if (!isInitState)
            //    PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
        }
        public void RemoveFromDerForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
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
        //NOT COMPLETE - Ovaj kod bi trebalo da se izmesti u CECalculationMicroservice
        public async Task CalculateNewFlexibility(DataToUI data)
        {
            //using (var tx = stateManager.CreateTransaction())
            //{
            //    Dictionary<DMSType, long> affectedDERForcast = new Dictionary<DMSType, long>();
            //    string type = "empty";

            //    nmsCache = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("nmsCache").Result;

            //    if (!await nmsCache.ContainsKeyAsync(tx, data.Gid))
            //    {
            //        type = nmsCache.TryGetValueAsync(tx, data.Gid).Result.Value.GetType().Name;
            //    }
            //    else
            //    {
            //        type = "NetworkModel";
            //    }

            //    Dictionary<long, IdentifiedObject> affectedEntities = new Dictionary<long, IdentifiedObject>();
            //    listOfGeneratorsForScada = new Dictionary<long, double>();

            //    DataToUI dataForScada = new DataToUI();
            //    copyOfProductionCached = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("copyOfProductionCached").Result;

            //    //copyOfProductionCached = new Dictionary<long, DerForecastDayAhead>(productionCached.Count); // TRENUTNA PROIZVODNJA 24 CASA UNAPRED

            //    /*foreach (DerForecastDayAhead der in productionCached)
            //    {
            //        await copyOfProductionCached.AddAsync(tx, der.entityGid, new DerForecastDayAhead(der));
            //    }*/

            //    productionCached = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("productionCached").Result;

            //    IAsyncEnumerable<KeyValuePair<long, DerForecastDayAhead>> productionCachedEnumerable = productionCached.CreateEnumerableAsync(tx).Result;
            //    using (IAsyncEnumerator<KeyValuePair<long, DerForecastDayAhead>> productionCachedEnumerator = productionCachedEnumerable.GetAsyncEnumerator())
            //    {
            //        while (productionCachedEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //        {
            //            await copyOfProductionCached.AddAsync(tx, productionCachedEnumerator.Current.Value.entityGid, new DerForecastDayAhead(productionCachedEnumerator.Current.Value));
            //        }
            //    }
            //    //OVO JE VALIDNO
            //    if (await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CheckFlexibilityForManualCommanding(data.Gid, nmsCache, stateManager)))
            //    {
            //        if (type.Equals("Generator"))
            //        {
            //            IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerable = nmsCache.CreateEnumerableAsync(tx).Result;
            //            using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerator = nmsCacheEnumerable.GetAsyncEnumerator())
            //            {
            //                while (nmsCacheEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //                {
            //                    if (nmsCacheEnumerator.Current.Value.GetType().Name.Equals("GeographicalRegion"))
            //                    {
            //                        GeographicalRegion gr = (GeographicalRegion)nmsCacheEnumerator.Current.Value;

            //                        foreach (long s in gr.Regions)
            //                        {
            //                            ConditionalValue<IdentifiedObject> subGeographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, s).Result;
            //                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)subGeographicalRegionIdentifiedObject.Value;

            //                            foreach (long sub in subGeographicalRegion.Substations)
            //                            {
            //                                ConditionalValue<IdentifiedObject> substationIdentifiedObject = nmsCache.TryGetValueAsync(tx, sub).Result;
            //                                Substation substation = (Substation)substationIdentifiedObject.Value;

            //                                if (substation.Equipments.Contains(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Generator
            //                                {
            //                                    if (nmsCache.TryGetValueAsync(tx, data.Gid).Result.GetType().Name.Equals("Generator"))
            //                                    {
            //                                        ConditionalValue<IdentifiedObject> generatorIdentifiedObject = nmsCache.TryGetValueAsync(tx, data.Gid).Result;
            //                                        Generator generator = (Generator)generatorIdentifiedObject.Value;

            //                                        if (!affectedEntities.ContainsKey(gr.GlobalId))
            //                                            affectedEntities.Add(gr.GlobalId, gr);

            //                                        if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
            //                                            affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

            //                                        if (!affectedEntities.ContainsKey(substation.GlobalId))
            //                                            affectedEntities.Add(substation.GlobalId, substation);

            //                                        if (!affectedEntities.ContainsKey(generator.GlobalId))
            //                                            affectedEntities.Add(generator.GlobalId, generator);
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewDerForecastDayAheadForGenerator(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities, stateManager));
            //            listOfGeneratorsForScada = await derFlexibility.InvokeWithRetryAsync(client => client.Channel.TurnOnFlexibilityForGenerator(data.Flexibility, data.Gid, affectedEntities));
            //        }
            //        else if (type.Equals("Substation"))
            //        {
            //            IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerable = nmsCache.CreateEnumerableAsync(tx).Result;
            //            using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerator = nmsCacheEnumerable.GetAsyncEnumerator())
            //            {
            //                while (nmsCacheEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //                {
            //                    if (nmsCacheEnumerator.Current.Value.GetType().Name.Equals("GeographicalRegion"))
            //                    {
            //                        GeographicalRegion gr = (GeographicalRegion)nmsCacheEnumerator.Current.Value;

            //                        foreach (long s in gr.Regions)
            //                        {
            //                            ConditionalValue<IdentifiedObject> subGeographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, s).Result;
            //                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)subGeographicalRegionIdentifiedObject.Value;

            //                            foreach (long sub in subGeographicalRegion.Substations)
            //                            {
            //                                ConditionalValue<IdentifiedObject> substationIdentifiedObject = nmsCache.TryGetValueAsync(tx, sub).Result;
            //                                Substation substation = (Substation)substationIdentifiedObject.Value;

            //                                if (substation.GlobalId.Equals(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Substation
            //                                {
            //                                    foreach (long gen in substation.Equipments)
            //                                    {
            //                                        if (nmsCache.TryGetValueAsync(tx, gen).Result.GetType().Name.Equals("Generator"))
            //                                        {
            //                                            ConditionalValue<IdentifiedObject> generatorIdentifiedObject = nmsCache.TryGetValueAsync(tx, gen).Result;
            //                                            Generator generator = (Generator)generatorIdentifiedObject.Value;

            //                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
            //                                                affectedEntities.Add(gr.GlobalId, gr);

            //                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
            //                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

            //                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
            //                                                affectedEntities.Add(substation.GlobalId, substation);

            //                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
            //                                                affectedEntities.Add(generator.GlobalId, generator);
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewDerForecastDayAheadForSubstation(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities, stateManager));
            //            listOfGeneratorsForScada = await derFlexibility.InvokeWithRetryAsync(client => client.Channel.TurnOnFlexibilityForSubstation(data.Flexibility, data.Gid, affectedEntities));
            //        }
            //        else if (type.Equals("SubGeographicalRegion"))
            //        {
            //            IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerable = nmsCache.CreateEnumerableAsync(tx).Result;
            //            using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerator = nmsCacheEnumerable.GetAsyncEnumerator())
            //            {
            //                while (nmsCacheEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //                {
            //                    if (nmsCacheEnumerator.Current.Value.GetType().Name.Equals("GeographicalRegion"))
            //                    {
            //                        GeographicalRegion gr = (GeographicalRegion)nmsCacheEnumerator.Current.Value;

            //                        foreach (long s in gr.Regions)
            //                        {
            //                            ConditionalValue<IdentifiedObject> subGeographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, s).Result;
            //                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)subGeographicalRegionIdentifiedObject.Value;

            //                            if (subGeographicalRegion.GlobalId.Equals(data.Gid))
            //                            {
            //                                foreach (long sub in subGeographicalRegion.Substations)
            //                                {
            //                                    ConditionalValue<IdentifiedObject> substationIdentifiedObject = nmsCache.TryGetValueAsync(tx, sub).Result;
            //                                    Substation substation = (Substation)substationIdentifiedObject.Value;

            //                                    foreach (long gen in substation.Equipments)
            //                                    {
            //                                        if (nmsCache.TryGetValueAsync(tx, gen).Result.GetType().Name.Equals("Generator"))
            //                                        {
            //                                            ConditionalValue<IdentifiedObject> generatorIdentifiedObject = nmsCache.TryGetValueAsync(tx, gen).Result;
            //                                            Generator generator = (Generator)generatorIdentifiedObject.Value;

            //                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
            //                                                affectedEntities.Add(gr.GlobalId, gr);

            //                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
            //                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

            //                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
            //                                                affectedEntities.Add(substation.GlobalId, substation);

            //                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
            //                                                affectedEntities.Add(generator.GlobalId, generator);
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewDerForecastDayAheadForSubGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities, stateManager));
            //            listOfGeneratorsForScada = await derFlexibility.InvokeWithRetryAsync(client => client.Channel.TurnOnFlexibilityForSubGeoRegion(data.Flexibility, data.Gid, affectedEntities));
            //        }
            //        else if (type.Equals("GeographicalRegion"))
            //        {
            //            ConditionalValue<IdentifiedObject> geographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, data.Gid).Result;
            //            GeographicalRegion gr = (GeographicalRegion)geographicalRegionIdentifiedObject.Value;

            //            foreach (long s in gr.Regions)
            //            {
            //                ConditionalValue<IdentifiedObject> subGeographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, s).Result;
            //                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)subGeographicalRegionIdentifiedObject.Value;

            //                foreach (long sub in subGeographicalRegion.Substations)
            //                {
            //                    ConditionalValue<IdentifiedObject> substationIdentifiedObject = nmsCache.TryGetValueAsync(tx, sub).Result;
            //                    Substation substation = (Substation)substationIdentifiedObject.Value;

            //                    foreach (long gen in substation.Equipments)
            //                    {
            //                        if (nmsCache.TryGetValueAsync(tx, gen).Result.GetType().Name.Equals("Generator"))
            //                        {
            //                            ConditionalValue<IdentifiedObject> generatorIdentifiedObject = nmsCache.TryGetValueAsync(tx, gen).Result;
            //                            Generator generator = (Generator)generatorIdentifiedObject.Value;

            //                            if (!affectedEntities.ContainsKey(gr.GlobalId))
            //                                affectedEntities.Add(gr.GlobalId, gr);

            //                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
            //                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

            //                            if (!affectedEntities.ContainsKey(substation.GlobalId))
            //                                affectedEntities.Add(substation.GlobalId, substation);

            //                            if (!affectedEntities.ContainsKey(generator.GlobalId))
            //                                affectedEntities.Add(generator.GlobalId, generator);
            //                        }
            //                    }
            //                }
            //            }
            //            await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewDerForecastDayAheadForGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities, stateManager));
            //            listOfGeneratorsForScada = await derFlexibility.InvokeWithRetryAsync(client => client.Channel.TurnOnFlexibilityForGeoRegion(data.Flexibility, data.Gid, affectedEntities));
            //        }
            //        else if (type.Equals("NetworkModel")) // OVAJ TREBA PROVERITI
            //        {
            //            IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerable = nmsCache.CreateEnumerableAsync(tx).Result;
            //            using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> nmsCacheEnumerator = nmsCacheEnumerable.GetAsyncEnumerator())
            //            {
            //                while (nmsCacheEnumerator.MoveNextAsync(CancellationToken.None).Result)
            //                {
            //                    if (nmsCacheEnumerator.Current.Value.GetType().Name.Equals("GeographicalRegion"))
            //                    {
            //                        GeographicalRegion gr = (GeographicalRegion)nmsCacheEnumerator.Current.Value;

            //                        foreach (long s in gr.Regions)
            //                        {
            //                            ConditionalValue<IdentifiedObject> subGeographicalRegionIdentifiedObject = nmsCache.TryGetValueAsync(tx, s).Result;
            //                            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)subGeographicalRegionIdentifiedObject.Value;

            //                            foreach (long sub in subGeographicalRegion.Substations)
            //                            {
            //                                ConditionalValue<IdentifiedObject> substationIdentifiedObject = nmsCache.TryGetValueAsync(tx, sub).Result;
            //                                Substation substation = (Substation)substationIdentifiedObject.Value;

            //                                if (substation.Equipments.Contains(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Generator
            //                                {
            //                                    if (nmsCache.TryGetValueAsync(tx, data.Gid).Result.GetType().Name.Equals("Generator"))
            //                                    {
            //                                        ConditionalValue<IdentifiedObject> generatorIdentifiedObject = nmsCache.TryGetValueAsync(tx, data.Gid).Result;
            //                                        Generator generator = (Generator)generatorIdentifiedObject.Value;

            //                                        if (!affectedEntities.ContainsKey(gr.GlobalId))
            //                                            affectedEntities.Add(gr.GlobalId, gr);

            //                                        if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
            //                                            affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

            //                                        if (!affectedEntities.ContainsKey(substation.GlobalId))
            //                                            affectedEntities.Add(substation.GlobalId, substation);

            //                                        if (!affectedEntities.ContainsKey(generator.GlobalId))
            //                                            affectedEntities.Add(generator.GlobalId, generator);
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            await derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewDerForecastDayAheadForNetworkModel(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities, stateManager));
            //            listOfGeneratorsForScada = await derFlexibility.InvokeWithRetryAsync(client => client.Channel.TurnOnFlexibilityForNetworkModel(data.Flexibility, data.Gid, affectedEntities));
            //        }
            //    }

            //    dataForScada.DataFromCEToScada = listOfGeneratorsForScada;

            //    await pubSub.InvokeWithRetryAsync(client => client.Channel.Notify(dataForScada, (int)Enums.Topics.Flexibility));

            //    //ClientSideCE.Instance.ProxyScadaListOfGenerators.SendListOfGenerators(listOfGeneratorsForScada);
            //    ApplyChangesOnProductionCached(); // OVU LINIJU OBRISATI I POZVATI JE KAD SKADA POSALJE ODGOVOR	
            //}
            /*
            Dictionary<DMSType, long> affectedDERForcast = new Dictionary<DMSType, long>();
            DERFlexibility flexibility = new DERFlexibility();
            string type = "empty";

            if (nmsCache.ContainsKey(data.Gid))
            {
                type = nmsCache[data.Gid].GetType().Name;
            }
            else
            {
                type = "NetworkModel";
            }

            Dictionary<long, IdentifiedObject> affectedEntities = new Dictionary<long, IdentifiedObject>();
            listOfGeneratorsForScada = new Dictionary<long, double>();
            DataToUI dataForScada = new DataToUI();

            copyOfProductionCached = new Dictionary<long, DerForecastDayAhead>(productionCached.Count); // TRENUTNA PROIZVODNJA 24 CASA UNAPRED

            foreach (DerForecastDayAhead der in productionCached.Values)
            {
                copyOfProductionCached.Add(der.entityGid, new DerForecastDayAhead(der));
            }

            if (flexibility.CheckFlexibilityForManualCommanding(data.Gid, nmsCache))
            {
                if (type.Equals("Generator"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];
                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    if (substation.Equipments.Contains(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Generator
                                    {
                                        if (nmsCache[data.Gid].GetType().Name.Equals("Generator"))
                                        {
                                            Generator generator = (Generator)nmsCache[data.Gid];

                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                affectedEntities.Add(gr.GlobalId, gr);

                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                affectedEntities.Add(substation.GlobalId, substation);

                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                affectedEntities.Add(generator.GlobalId, generator);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForGenerator(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForGenerator(data.Flexibility, data.Gid, affectedEntities);
                }
                else if (type.Equals("Substation"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];
                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];
                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    if (substation.GlobalId.Equals(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Substation
                                    {
                                        foreach (long gen in substation.Equipments)
                                        {
                                            if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)nmsCache[gen];

                                                if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                    affectedEntities.Add(gr.GlobalId, gr);

                                                if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                    affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                                if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                    affectedEntities.Add(substation.GlobalId, substation);

                                                if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                    affectedEntities.Add(generator.GlobalId, generator);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForSubstation(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForSubstation(data.Flexibility, data.Gid, affectedEntities);
                }
                else if (type.Equals("SubGeographicalRegion"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];

                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                if (subGeographicalRegion.GlobalId.Equals(data.Gid))
                                {

                                    foreach (long sub in subGeographicalRegion.Substations)
                                    {
                                        Substation substation = (Substation)nmsCache[sub];

                                        foreach (long gen in substation.Equipments)
                                        {
                                            if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)nmsCache[gen];

                                                if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                    affectedEntities.Add(gr.GlobalId, gr);

                                                if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                    affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                                if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                    affectedEntities.Add(substation.GlobalId, substation);

                                                if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                    affectedEntities.Add(generator.GlobalId, generator);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForSubGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForSubGeoRegion(data.Flexibility, data.Gid, affectedEntities);

                }
                else if (type.Equals("GeographicalRegion"))
                {

                    GeographicalRegion gr = (GeographicalRegion)nmsCache[data.Gid];

                    foreach (long s in gr.Regions)
                    {
                        SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                        foreach (long sub in subGeographicalRegion.Substations)
                        {
                            Substation substation = (Substation)nmsCache[sub];

                            foreach (long gen in substation.Equipments)
                            {
                                if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                {
                                    Generator generator = (Generator)nmsCache[gen];

                                    if (!affectedEntities.ContainsKey(gr.GlobalId))
                                        affectedEntities.Add(gr.GlobalId, gr);

                                    if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                        affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                    if (!affectedEntities.ContainsKey(substation.GlobalId))
                                        affectedEntities.Add(substation.GlobalId, substation);

                                    if (!affectedEntities.ContainsKey(generator.GlobalId))
                                        affectedEntities.Add(generator.GlobalId, generator);
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForGeoRegion(data.Flexibility, data.Gid, affectedEntities);

                }
                else if (type.Equals("NetworkModel"))
                {
                    foreach (var grType in nmsCache.Values)
                    {
                        if (grType.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)grType;

                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    foreach (long gen in substation.Equipments)
                                    {
                                        if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                        {
                                            Generator generator = (Generator)nmsCache[gen];

                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                affectedEntities.Add(gr.GlobalId, gr);

                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                affectedEntities.Add(substation.GlobalId, substation);

                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                affectedEntities.Add(generator.GlobalId, generator);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForNetworkModel(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForNetworkModel(data.Flexibility, data.Gid, affectedEntities);
                }
            }

            //dataForScada.DataFromCEToScada = listOfGeneratorsForScada;
            //PubSubCalculatioEngine.Instance.Notify(dataForScada, (int)Enums.Topics.Flexibility);

            ClientSideCE.Instance.ProxyScadaListOfGenerators.SendListOfGenerators(listOfGeneratorsForScada);
            ApplyChangesOnProductionCached(); // OVU LINIJU OBRISATI I POZVATI JE KAD SKADA POSALJE ODGOVOR		*/
        }
        //Poziva se iz metode koja ne bi trebalo da stoji u cache-u/
        public async void ApplyChangesOnProductionCached() // KAD STIGNE POTVRDA SA SKADE DA SU PROMENE IZVRSENE, POZIVAMO OVU METODU KAKO BI NOVI PRORACUNI PROIZVODNJE ZA 24h BILI PRIMENJENI NA CACHE
        {
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, DerForecastDayAhead> ProductionCachedDictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                IReliableDictionary<long, DerForecastDayAhead> CopyOfProductionCachedDictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;

                ProductionCachedDictionary.ClearAsync();

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

            SendDerForecastDayAhead();
            UpdateMinAndMaxFlexibilityForChangedGenerators();
        }
        //NOT COMPLETE PubSubCalculatioEngine *- Trebalo bi da se izmeni ako se bude zvalo iz nekog drugog servisa.
        public async Task SendDerForecastDayAhead()
        {
            //PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
            await pubSub.InvokeWithRetryAsync(client => client.Channel.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead));
        }
        //NOT COMPLETE CEUpdateThroughUI missing, PubSubCalculatioEngine
        public async Task<float> PopulateBalance(long gid)
        {
            CloudClient<ICEUpdateThroughUI> transactionCoordinator = new CloudClient<ICEUpdateThroughUI>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: ServicePartitionKey.Singleton,
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CEUpdateThroughUIServiceListener"
            );
            Dictionary<long, DerForecastDayAhead> productionCachedDictionary = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, IdentifiedObject> nmsCacheDictionary = new Dictionary<long, IdentifiedObject>();
            List<long> turnedOffGeneratorsList = new List<long>();
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
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<long, IdentifiedObject> dict = stateManager.GetOrAddAsync<IReliableDictionary<long, IdentifiedObject>>("NmsCacheDictionary").Result;

                IAsyncEnumerable<KeyValuePair<long, IdentifiedObject>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<long, IdentifiedObject>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        nmsCacheDictionary.Add(dictEnumerator.Current.Key, dictEnumerator.Current.Value);
                    }
                }
            }
            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("turnedOffGeneratorsList").Result;

                IAsyncEnumerable<KeyValuePair<int, List<long>>> dictEnumerable = dict.CreateEnumerableAsync(tx).Result;
                using (IAsyncEnumerator<KeyValuePair<int, List<long>>> dictEnumerator = dictEnumerable.GetAsyncEnumerator())
                {
                    while (dictEnumerator.MoveNextAsync(CancellationToken.None).Result)
                    {
                        //-->>>PROVERITI DA LI JE OVA LISTA DOBRO POPUNJENA
                       turnedOffGeneratorsList=dictEnumerator.Current.Value;
                    }
                }
            }
            float energyFromSource = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.Balance(productionCachedDictionary, gid, nmsCacheDictionary, turnedOffGeneratorsList));
            //PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
            //return energyFromSource;
            return 0;
        }
        //Ne koristi se nigde
        //public DerForecastDayAhead GetDerForecastDayAhead(long gid)
        //{
        //    //if (!productionCached.ContainsKey(gid))
        //    //    return null;
        //    //return productionCached[gid];
        //    return null;
        //}
        public DataToUI CreateDataForUI()
        {
            Dictionary<long, DerForecastDayAhead> tempDictionary = new Dictionary<long, DerForecastDayAhead>();
            DataToUI data = new DataToUI();

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
                data.Data = tempDictionary;
                return data;
            }
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
        public void AddDerForecastDayAhead(long id, DerForecastDayAhead forecast)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, id, forecast, (key, value) => value = forecast);
                tx.CommitAsync();
            }

        }
        public void RemoveFromDerForecastDayAhead(long id)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("ProductionCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, id);
                tx.CommitAsync();
            }

        }
        #endregion

        #region TreeGraph Methods
        //NOT COMPLETE CALC FLEXIBILITY treba da se pozove odavde //CalculateFlexibility -- calc flexibility bi trebalo da stoji u nekom drugom servisu
        private async void PopulateGraph(NetworkModelTransfer networkModelTransfer)
        {
            CloudClient<ITreeConstruction> transactionCoordinator = new CloudClient<ITreeConstruction>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "BuildTreeServiceListener"
            );

            TreeNode<NodeData> graph = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.ConstructTree1(networkModelTransfer));

            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result;
                await dictionary.AddOrUpdateAsync(tx, 0, graph, (key, value) => value = graph);
                await tx.CommitAsync();
            }

            // ovaj deo koda nema smisla jer je graf vec tu iznad
            //networkModelTreeClass = await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNetworkModelTreeClass());

            //CalculateFlexibility();
        }
        public async void UpdateGraphWithScadaValues(List<DataPoint> data)
        {
            CloudClient<ITreeConstruction> transactionCoordinator = new CloudClient<ITreeConstruction>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: ServicePartitionKey.Singleton,
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "BuildTreeServiceListener"
            );

            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result;
                //Upitno da li radi ovaj upit
                TreeNode<NodeData> graphToSend = stateManager.GetOrAddAsync<IReliableDictionary<int, TreeNode<NodeData>>>("GraphCachedDictionary").Result.TryGetValueAsync(tx, 0).Result.Value;
                TreeNode<NodeData> graph = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.UpdateGraphWithScadaValues(data, graphToSend)).Result;
                await dictionary.AddOrUpdateAsync(tx, 0, graph, (key, value) => value = graph);
                await tx.CommitAsync();
            }
        }
        public TreeNode<NodeData> GetGraph()
        {
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
        public async void UpdateNewDataPoitns(List<DataPoint> points)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;
                List<DataPoint> dataPoints = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                if (dataPoints == null)
                {
                    dataPoints = new List<DataPoint>();
                    await dictionary.AddOrUpdateAsync(tx, 0, dataPoints, (key, value) => value = dataPoints);
                    await tx.CommitAsync();
                }

                foreach (DataPoint data in points)
                {
                    foreach (DataPoint dp in dataPoints)
                    {
                        //
                        if (dataPoints.Where(x => x.Gid == data.Gid).Count() == 0)
                        {
                            dataPoints.Add(data);
                        }
                        else
                        {
                            dataPoints[dataPoints.FindIndex(ind => ind.Gid == data.Gid)] = data;
                        }
                        //
                    }
                    await dictionary.AddOrUpdateAsync(tx, 0, dataPoints, (key, value) => value = dataPoints);
                    await tx.CommitAsync();
                }
            }
        }
        public List<DataPoint> GetDatapoints()
        {
            List<DataPoint> dPoints = new List<DataPoint>();

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;

                dPoints = dict.TryGetValueAsync(tx, 0).Result.Value;
            }

            return dPoints;
        }
        public void AddToDataPoints(DataPoint datapoint)
        {

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;
                List<DataPoint> points = dict.TryGetValueAsync(tx, 0).Result.Value;
                points.Add(datapoint);

                dict.AddOrUpdateAsync(tx, 0, points, (key, value) => value = points);
            }
        }
        public void RemoveFromDataPoints(DataPoint datapoint)
        {

            using (var tx = stateManager.CreateTransaction())
            {
                IReliableDictionary<int, List<DataPoint>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<DataPoint>>>("DataPointsCachedDictionary").Result;
                List<DataPoint> points = dict.TryGetValueAsync(tx, 0).Result.Value;
                points.Remove(datapoint);

                dict.AddOrUpdateAsync(tx, 0, points, (key, value) => value = points);
            }
        }
        #endregion

        #region copyOfProductionCached methods
        //ApplyChangesOnProductionCached
        //CalculateNewFlexibility -- vec postoje 
        public Dictionary<long, DerForecastDayAhead> GetCopyOfProductionCached()
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
        public void AddToCopyOfProductionCached(long gid, DerForecastDayAhead forecast)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, forecast, (key, value) => value = forecast);
                tx.CommitAsync();
            }
        }
        public void RemoveFromCopyOfProductionCached(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("CopyOfProductionCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

        #region listOfGeneratorsForScada methods
        //CalculateNewFlexibility
        //CHECK DA LI TREBA DA SE UBACE U interfejs
        //NOT COMPLETE -- Trebalo bi da stoje na nekom drugom servisu
        public async Task UpdateMinAndMaxFlexibilityForChangedGenerators()
        {
            //double minProd = 0;
            //double maxProd = 0;
            //double currentProd = 0;

            //foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModelTreeClass)
            //{
            //    foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
            //    {
            //        foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
            //        {
            //            foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
            //            {
            //                foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
            //                {
            //                    if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
            //                    {
            //                        if (listOfGeneratorsForScada.ContainsKey(substationElementTreeClass.GID))
            //                        {
            //                            maxProd = substationElementTreeClass.P + substationElementTreeClass.P * (substationElementTreeClass.MaxFlexibility / 100);
            //                            minProd = substationElementTreeClass.P - substationElementTreeClass.P * (substationElementTreeClass.MinFlexibility / 100);

            //                            currentProd = substationElementTreeClass.P + substationElementTreeClass.P * (listOfGeneratorsForScada[substationElementTreeClass.GID] / 100);

            //                            substationElementTreeClass.P = (float)currentProd;
            //                            substationElementTreeClass.MaxFlexibility = (float)(((maxProd - currentProd) * 100) / currentProd);
            //                            substationElementTreeClass.MinFlexibility = (float)(((currentProd - minProd) * 100) / currentProd);

            //                            IdentifiedObject gen = nmsCache[substationElementTreeClass.GID];
            //                            ((Generator)gen).ConsiderP = substationElementTreeClass.P;
            //                            ((Generator)gen).MaxFlexibility = substationElementTreeClass.MaxFlexibility;
            //                            ((Generator)gen).MinFlexibility = substationElementTreeClass.MinFlexibility;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //CalculateFlexibility();
        }
        //NOT COMPLETE -- Trebalo bi da stoje na nekom drugom servisu
        private void CalculateFlexibility()
        {
            //float minFlexibilitySubstation = 0;
            //float maxFlexibilitySubstation = 0;
            //float productionSubstation = 0;

            //float minFlexibilitySubRegion = 0;
            //float maxFlexibilitySubRegion = 0;
            //float productionSubRegion = 0;

            //float minFlexibilityGeoRegion = 0;
            //float maxFlexibilityGeoRegion = 0;
            //float productionGeoRegion = 0;

            //float minFlexibilityNetworkModel = 0;
            //float maxFlexibilityNetworkModel = 0;
            //float productionNetworkModel = 0;

            //foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModelTreeClass)
            //{
            //    foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
            //    {
            //        foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
            //        {
            //            foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
            //            {
            //                foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
            //                {
            //                    if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
            //                    {
            //                        productionSubstation += substationElementTreeClass.P;
            //                        minFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MinFlexibility) / 100;
            //                        maxFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MaxFlexibility) / 100;
            //                    }
            //                }

            //                substationTreeClass.MinFlexibility = (100 * minFlexibilitySubstation) / productionSubstation;
            //                substationTreeClass.MaxFlexibility = (100 * maxFlexibilitySubstation) / productionSubstation;

            //                productionSubRegion += productionSubstation;
            //                minFlexibilitySubRegion += (productionSubstation * substationTreeClass.MinFlexibility) / 100;
            //                maxFlexibilitySubRegion += (productionSubstation * substationTreeClass.MaxFlexibility) / 100;

            //                minFlexibilitySubstation = 0;
            //                maxFlexibilitySubstation = 0;
            //                productionSubstation = 0;
            //            }

            //            geographicalSubRegionTreeClass.MinFlexibility = (100 * minFlexibilitySubRegion) / productionSubRegion;
            //            geographicalSubRegionTreeClass.MaxFlexibility = (100 * maxFlexibilitySubRegion) / productionSubRegion;

            //            productionGeoRegion += productionSubRegion;
            //            minFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MinFlexibility) / 100;
            //            maxFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MaxFlexibility) / 100;

            //            minFlexibilitySubRegion = 0;
            //            maxFlexibilitySubRegion = 0;
            //            productionSubRegion = 0;
            //        }

            //        geographicalRegionTreeClass.MinFlexibility = (100 * minFlexibilityGeoRegion) / productionGeoRegion;
            //        geographicalRegionTreeClass.MaxFlexibility = (100 * maxFlexibilityGeoRegion) / productionGeoRegion;

            //        productionNetworkModel += productionGeoRegion;
            //        minFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MinFlexibility) / 100;
            //        maxFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MaxFlexibility) / 100;

            //        minFlexibilityGeoRegion = 0;
            //        maxFlexibilityGeoRegion = 0;
            //        productionGeoRegion = 0;

            //    }

            //    networkModelTreeClasses.MinFlexibility = (100 * minFlexibilityNetworkModel) / productionNetworkModel;
            //    networkModelTreeClasses.MaxFlexibility = (100 * maxFlexibilityNetworkModel) / productionNetworkModel;

            //    minFlexibilityNetworkModel = 0;
            //    maxFlexibilityNetworkModel = 0;
            //    productionNetworkModel = 0;
            //}

            //upisati vrednosti
            //DataToUI data = new DataToUI();
            //data.NetworkModelTreeClass = NetworkModelTreeClass;
            ////PubSubCalculatioEngine.Instance.Notify(data, (int)Enums.Topics.NetworkModelTreeClass);
            // await pubSub.InvokeWithRetryAsync(client => client.Channel.Notify(data, (int)Enums.Topics.NetworkModelTreeClass));
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
        public void AddToListOfGeneratorsForScada(long gid, double param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, double>>("ListOfGeneratorsForScadaCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
            }
        }
        public void RemoveFromListOfGeneratorsForScada(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, double>>("ListOfGeneratorsForScadaCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

        #region DisableAutomaticOptimization methods
        //CHECK DA LI TREBA DA SE UBACE U interfejs
        //NOT COMPLETE 
        //ListOfDisabledGenerators
        //AllowOptimization
        //ListOffTurnedOffGenerators -- Stoje negde drugde ali traze vrednosti te liste, ali ne stoje u cache
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
        public void AddToDisableAutomaticOptimization(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Add(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
            }
        }
        public void RemoveFromDisableAutomaticOptimization(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("DisableAutomaticOptimizationCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Remove(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
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
                IReliableDictionary<int, List<long>> dict = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;

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
                lista.Add(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
            }
        }
        public async Task RemoveFromTurnedOffGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOffGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Remove(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
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
                lista.Add(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
            }
        }
        public async Task RemoveFromTurnedOnGenerators(long param)
        {
            using (var tx = stateManager.CreateTransaction())
            {

                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<int, List<long>>>("TurnedOnGeneratorsCachedDictionary").Result;
                List<long> lista = dictionary.TryGetValueAsync(tx, 0).Result.Value;
                lista.Remove(param);
                dictionary.AddOrUpdateAsync(tx, 0, lista, (key, value) => value = lista);
                tx.CommitAsync();
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
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
            }
        }
        public async Task RemoveFromTempProductionCached(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("TempProductionCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
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
        public void AddToSubstationDayAhead(long gid, DayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DayAhead>>("SubstationDayAheadCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
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
        public Dictionary<long, DerForecastDayAhead> GetSubstationsForecast() //SubstationsForecastCachedDictionary
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
        public void AddToSubstationsForecast(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubstationsForecastCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
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
        public Dictionary<long, DerForecastDayAhead> GetSubGeographicalRegionsForecast() // SubGeographicalRegionsForecastCachedDictionary
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
        public void AddToSubGeographicalRegionsForecast(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
            }
        }
        public void RemoveFromSubGeographicalRegionsForecast(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("SubGeographicalRegionsForecastCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

        #region generatorForecastList methods
        //PopulateProductionForecast GORE U KODU, tamo treba implementirati
        public Dictionary<long, DerForecastDayAhead> GetGeneratorForecastList() // GeneratorForecastListCachedDictionary
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
        public void AddToGeneratorForecastList(long gid, DerForecastDayAhead param)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;
                dictionary.AddOrUpdateAsync(tx, gid, param, (key, value) => value = param);
                tx.CommitAsync();
            }
        }
        public void RemoveFromGeneratorForecastList(long gid)
        {
            using (var tx = stateManager.CreateTransaction())
            {
                var dictionary = stateManager.GetOrAddAsync<IReliableDictionary<long, DerForecastDayAhead>>("GeneratorForecastListCachedDictionary").Result;
                dictionary.TryRemoveAsync(tx, gid);
                tx.CommitAsync();
            }
        }
        #endregion

    }
}
