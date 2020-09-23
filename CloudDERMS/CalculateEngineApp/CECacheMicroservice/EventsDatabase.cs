using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using CloudCommon.SCADA;
using CloudCommon.SCADA.AzureStorage;
using CloudCommon.SCADA.AzureStorage.Entities;
using DERMSCommon;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECacheMicroservice
{
    public class EventsDatabase : IEvetnsDatabase
    {
        public async Task<List<Event>> GetEvents()
        {
            List<EventStorage> eventStorages = AzureTableStorage.GetAllEventStorageItems("UseDevelopmentStorage=true;", "EventsStorage");
            List<Event> events = new List<Event>();

            foreach (EventStorage eventStorage in eventStorages)
            {
                events.Add(new Event(eventStorage.Message, eventStorage.Component, eventStorage.Timestamp.DateTime));
            }

            return events;
        }

        public async Task SetEvent(Event eventt)
        {
            EventStorage eventStorage = new EventStorage(eventt.Message, eventt.Component, eventt.DateTime);
            List<EventStorage> eventStorages = new List<EventStorage>() { eventStorage };
            bool allGood = AzureTableStorage.InsertEntitiesInDB(eventStorages, "UseDevelopmentStorage=true;", "EventsStorage");

            if (allGood)
            {
                CloudClient<IPubSub> pubSub = new CloudClient<IPubSub>
                (
                  serviceUri: new Uri("fabric:/CalculateEngineApp/CEPubSubMicroservice"),
                  partitionKey: new ServicePartitionKey(0), /*CJN*/
                  clientBinding: WcfUtility.CreateTcpClientBinding(),
                  listenerName: "CEPubSubMicroServiceListener"
                );

                pubSub.InvokeWithRetryAsync(client => client.Channel.NotifyEvents(eventt, (int)Enums.Topics.Events)).Wait();
            }
        }
    }
}
