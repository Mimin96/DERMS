using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon.CalculateEngine;
using DERMSCommon;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace CEPubSubMicroservice
{
	public class CEPubSubService : IPubSub
	{
		private IReliableDictionary<string, ServerSideProxy> subscribers;
		private TopicSubscriptions topicSubscriptions;

		private IReliableStateManager stateManager;
		private StatefulServiceContext context;

		public CEPubSubService(IReliableStateManager StateManager, StatefulServiceContext context)
		{
			this.context = context;
			this.stateManager = StateManager;
			topicSubscriptions = new TopicSubscriptions(stateManager);
		}

		public async Task<bool> SubscribeSubscriber(string clientAddress, int gidOfTopic)
		{
			using (var tx = stateManager.CreateTransaction())
			{
				subscribers = stateManager.GetOrAddAsync<IReliableDictionary<string, ServerSideProxy>>("subscribers").Result;

				if (!await subscribers.ContainsKeyAsync(tx, clientAddress))
				{
					await subscribers.AddAsync(tx, clientAddress, new ServerSideProxy(clientAddress));
				}
				await tx.CommitAsync();

				await topicSubscriptions.SubscribeAsync(clientAddress, gidOfTopic);
			}
			DataToUI data = new DataToUI();
			data.Flexibility = 250;
			data.Gid = 225883;
			return await Notify(data, 1);
		}

		public async Task Unsubscribe(string clientAddress, long gidOfTopic, bool disconnect)
		{
			if (disconnect)
			{
				using (var tx = this.stateManager.CreateTransaction())
				{
					subscribers = stateManager.GetOrAddAsync<IReliableDictionary<string, ServerSideProxy>>("subscribers").Result;
					await subscribers.TryRemoveAsync(tx, clientAddress);
					await tx.CommitAsync();
				}
			}

			topicSubscriptions.Unsubscribe(clientAddress, gidOfTopic);
		}

		public async Task<bool> Notify(DataToUI forcastDayAhead, long gidOfTopic)
		{
			if (forcastDayAhead == null)
			{
				return false;
			}

			Dictionary<string, ServerSideProxy> subscribersCopy;
			List<string> deadClients = new List<string>();
			subscribersCopy = GetSubscribersCopy();

			List<string> topicSubscribers = topicSubscriptions.GetSubscribers(gidOfTopic);
			List<string> deadSubscribers = new List<string>();
			foreach (string subscriberAddress in topicSubscribers)
			{
				if (subscribersCopy.ContainsKey(subscriberAddress))
				{
					ServerSideProxy subscriberProxy = subscribersCopy[subscriberAddress];
					try
					{
						subscriberProxy.Proxy.SendDataToUI(forcastDayAhead);
						return true;
					}
					catch (CommunicationException)
					{
						if (RetrySendForecast(subscriberProxy, forcastDayAhead) == false)
						{
							deadClients.Add(subscriberAddress);
						}
						else
						{
							return true;
						}
					}
					catch (TimeoutException)
					{
						if (RetrySendForecast(subscriberProxy, forcastDayAhead) == false)
						{
							deadClients.Add(subscriberAddress);
						}
						else
						{
							return true;
						}
					}
				}
				else
				{
					deadSubscribers.Add(subscriberAddress);
				}

				await topicSubscriptions.RemoveDeadSubscribersForTopicAsync(gidOfTopic, deadSubscribers);
			}

			await RemoveDeadClients(deadClients);
			return false;
		}

		private async Task RemoveDeadClients(List<string> deadClients)
		{
			using (var tx = this.stateManager.CreateTransaction())
			{
				subscribers = stateManager.GetOrAddAsync<IReliableDictionary<string, ServerSideProxy>>("subscribers").Result;
				foreach (string clientAddress in deadClients)
				{
					await subscribers.TryRemoveAsync(tx, clientAddress);
				}

				await tx.CommitAsync();
			}
		}

		private bool RetrySendForecast(ServerSideProxy proxy, DataToUI forecast)
		{
			proxy.Abort();
			proxy.Connect();
			try
			{
				proxy.Proxy.SendDataToUI(forecast);
			}
			catch (CommunicationException e)
			{
				return false;
			}
			return true;
		}

		private Dictionary<string, ServerSideProxy> GetSubscribersCopy()
		{
			Dictionary<string, ServerSideProxy> subscribersCopy = new Dictionary<string, ServerSideProxy>();

			using (var tx = this.stateManager.CreateTransaction())
			{
				subscribers = stateManager.GetOrAddAsync<IReliableDictionary<string, ServerSideProxy>>("subscribers").Result;
				IAsyncEnumerable<KeyValuePair<string, ServerSideProxy>> subscribersEnumerable = subscribers.CreateEnumerableAsync(tx).Result;
				using (IAsyncEnumerator<KeyValuePair<string, ServerSideProxy>> subcriberEnumerator = subscribersEnumerable.GetAsyncEnumerator())
				{
					while (subcriberEnumerator.MoveNextAsync(CancellationToken.None).Result)
					{
						subscribersCopy.Add(subcriberEnumerator.Current.Key, subcriberEnumerator.Current.Value);
					}
				}
			}

			return subscribersCopy;
		}
	}
}
