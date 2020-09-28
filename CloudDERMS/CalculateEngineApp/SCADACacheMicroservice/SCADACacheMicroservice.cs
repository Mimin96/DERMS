using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon.SCADA;
using CloudCommon.SCADA.AzureStorage;
using CloudCommon.SCADA.AzureStorage.Entities;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SCADACacheMicroservice.MockDatabaseData;

namespace SCADACacheMicroservice
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SCADACacheMicroservice : StatefulService
    {
        public SCADACacheMicroservice(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            SendDataFromNmsToScada sendDataFromNMSToCE = new SendDataFromNmsToScada(StateManager);

            return new[]
            {
                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<ISendDataFromNMSToScada>(
                        wcfServiceObject: sendDataFromNMSToCE,
                        serviceContext: context,
                        endpointResourceName: "SCADACacheMicroserviceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "SCADACacheMicroserviceListener"
                ),
                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<IScadaCloudToScadaLocal>(
                        wcfServiceObject: new CloudScadaToLocalScada(StateManager),
                        serviceContext: context,
                        endpointResourceName: "SCADAComunicationMicroserviceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    ),
                    name: "SCADAComunicationMicroserviceListener"
                ),
                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<IScadaCloudToScadaLocal>(
                        wcfServiceObject: new CloudScadaToLocalScada(StateManager),
                        serviceContext: context,
                        address: new EndpointAddress("net.tcp://localhost:52358/SCADACacheMicroservice"),
                        listenerBinding: new NetTcpBinding()
                    ),
                    name: "SCADAComunicationMicroserviceLocalListener"
                ),
                new ServiceReplicaListener((context) =>
                    new WcfCommunicationListener<IHistoryDatabase>(
                        wcfServiceObject: new HistoryDatabase(),
                        serviceContext: context,
                        address: new EndpointAddress("net.tcp://localhost:52399/SCADACacheMicroservice"),
                        listenerBinding: new NetTcpBinding()
                    ),
                    name: "HistoryDatabaseListener"
                )
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            // Start_MockDatabase

            // MockHistoryDatabaseData historyDatabaseData = new MockHistoryDatabaseData();
            // historyDatabaseData.SetDatabaseData().Wait();

            // End_MockDatabase

            long millisecondInDay = 86400000;
            long millisecondInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) * millisecondInDay;
            long currentMillisecondsInADay = DateTime.Now.Hour * 3600000 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
            long currentMillisecondsInAMont = DateTime.Now.Day * millisecondInDay + currentMillisecondsInADay;

            Timer timerForDayItem = new Timer(x =>
            {
                CalculateDayItem();
            }, null, millisecondInDay - currentMillisecondsInADay + 30000, Timeout.Infinite);

            Timer timerForMonthItem = new Timer(x =>
            {
                CalculateMonthItem();
            }, null, millisecondInMonth - currentMillisecondsInAMont + 350000, Timeout.Infinite);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        Timer timerForDayItemTest;
        Timer timerForMonthItemTest;
        private void CalculateDayItem()
        {
            HistoryDataProcessing historyDataProcessing = new HistoryDataProcessing();
            DateTime dateTime = DateTime.Now;

            dateTime = dateTime.AddDays(-1);

            List<CollectItem> collectItems = AzureTableStorage.GetCollectItems("UseDevelopmentStorage=true;", "CollectItems", dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day);
            Dictionary<long, List<CollectItem>> byGids = new Dictionary<long, List<CollectItem>>();

            foreach (CollectItem item in collectItems)
            {
                if (!byGids.ContainsKey(item.Gid))
                    byGids.Add(item.Gid, new List<CollectItem>() { item });
                else
                {
                    List<CollectItem> items = byGids[item.Gid];
                    items.Add(item);
                    byGids[item.Gid] = items;
                }
            }

            foreach (long gid in byGids.Keys)
            {
                DayItem dayItem = historyDataProcessing.CollectTableToDayItems(byGids[gid]);
                List<DayItem> dayItems = new List<DayItem>() { dayItem };

                AzureTableStorage.InsertEntitiesInDB(dayItems, "UseDevelopmentStorage=true;", "DayItems");
            }

            long millisecondInDay = 86400000;
            long currentMillisecondsInADay = DateTime.Now.Hour * 3600000 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;

            timerForDayItemTest = new Timer(x =>
            {
                CalculateDayItem();
            }, null, millisecondInDay - currentMillisecondsInADay + 30000, Timeout.Infinite);
        }

        private void CalculateMonthItem()
        {
            HistoryDataProcessing historyDataProcessing = new HistoryDataProcessing();
            DateTime dateTime = DateTime.Now;

            if (dateTime.Day == 1)
            {
                dateTime = dateTime.AddDays(-1);
            }

            List<DayItem> dayItems = AzureTableStorage.GetDayItems("UseDevelopmentStorage=true;", "DayItems", dateTime.Year + "-" + dateTime.Month);
            Dictionary<long, List<DayItem>> byGids = new Dictionary<long, List<DayItem>>();

            foreach (DayItem item in dayItems)
            {
                if (!byGids.ContainsKey(item.Gid))
                    byGids.Add(item.Gid, new List<DayItem>() { item });
                else
                {
                    List<DayItem> items = byGids[item.Gid];
                    items.Add(item);
                    byGids[item.Gid] = items;
                }
            }

            foreach (long gid in byGids.Keys)
            {
                MonthItem monthItem = historyDataProcessing.DayItemsToMonthItems(byGids[gid]);
                List<MonthItem> monthItems = new List<MonthItem>() { monthItem };

                AzureTableStorage.InsertEntitiesInDB(monthItems, "UseDevelopmentStorage=true;", "MonthItems");
            }

            long millisecondInDay = 86400000;
            long millisecondInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) * millisecondInDay;
            long currentMillisecondsInADay = DateTime.Now.Hour * 3600000 + DateTime.Now.Minute * 60000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
            long currentMillisecondsInAMont = DateTime.Now.Day * millisecondInDay + currentMillisecondsInADay;

            timerForMonthItemTest = new Timer(x =>
            {
                CalculateMonthItem();
            }, null, millisecondInMonth - currentMillisecondsInAMont + 350000, Timeout.Infinite);

            if (DateTime.Now.Month == 1 && DateTime.Now.Day == 1)
                CalculateYearItem();
        }

        private void CalculateYearItem()
        {
            HistoryDataProcessing historyDataProcessing = new HistoryDataProcessing();
            DateTime dateTime = DateTime.Now;

            if (dateTime.Day == 1 && dateTime.Month == 1)
            {
                dateTime = dateTime.AddDays(-1);
            }

            List<MonthItem> monthItems = AzureTableStorage.GetMonthItems("UseDevelopmentStorage=true;", "MonthItems", dateTime.Year.ToString());
            Dictionary<long, List<MonthItem>> byGids = new Dictionary<long, List<MonthItem>>();

            foreach (MonthItem item in monthItems)
            {
                if (!byGids.ContainsKey(item.Gid))
                    byGids.Add(item.Gid, new List<MonthItem>() { item });
                else
                {
                    List<MonthItem> items = byGids[item.Gid];
                    items.Add(item);
                    byGids[item.Gid] = items;
                }
            }


            foreach (long gid in byGids.Keys)
            {
                YearItem yearItem = historyDataProcessing.MonthItemsToYearItems(byGids[gid]);
                List<YearItem> yearItems = new List<YearItem>() { yearItem };

                AzureTableStorage.InsertEntitiesInDB(yearItems, "UseDevelopmentStorage=true;", "YearItems");
            }

        }
    }
}
