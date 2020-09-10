﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;

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

			bool notFirstTime = false;
			using (var tx = stateManager.CreateTransaction())
			{
				IReliableQueue<bool> reliableQueue = stateManager.GetOrAddAsync<IReliableQueue<bool>>("notFirstTime").Result;
				notFirstTime = reliableQueue.TryPeekAsync(tx).Result.Value;
			}

			if (notFirstTime && (int)Enums.Topics.NetworkModelTreeClass_NodeData == gidOfTopic)
			{
				CloudClient<ICache> cache = new CloudClient<ICache>
				(
					  serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
					  partitionKey: new ServicePartitionKey(0),
					  clientBinding: WcfUtility.CreateTcpClientBinding(),
					  listenerName: "CECacheServiceListener"
				);
				TreeNode<NodeData> tree = cache.InvokeWithRetryAsync(client => client.Channel.GetGraph()).Result; ; 
				List<NetworkModelTreeClass> NetworkModelTreeClass = cache.InvokeWithRetryAsync(client => client.Channel.GetNetworkModelTreeClass()).Result;
				List<DataPoint> dataPoints = cache.InvokeWithRetryAsync(client => client.Channel.GetDatapoints()).Result;

				await NotifyTree(tree, NetworkModelTreeClass, (int)Enums.Topics.NetworkModelTreeClass_NodeData);
				await NotifyDataPoint(dataPoints, (int)Enums.Topics.DataPoints);
			}

			if ((int)Enums.Topics.NetworkModelTreeClass_NodeData == gidOfTopic) 
			{
				using (var tx = stateManager.CreateTransaction())
				{
					IReliableQueue<bool> reliableQueue = stateManager.GetOrAddAsync<IReliableQueue<bool>>("notFirstTime").Result;
					await reliableQueue.TryDequeueAsync(tx);
					await reliableQueue.EnqueueAsync(tx, true);

					await tx.CommitAsync();
				}
			}

			return true;
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
				proxy.Proxy.SendScadaDataToUI(forecast);
			}
			catch (CommunicationException e)
			{
				return false;
			}
			return true;
		}

		private bool RetrySendDataPoint(ServerSideProxy proxy, List<DataPoint> data)
		{
			proxy.Abort();
			proxy.Connect();
			try
			{
				proxy.Proxy.SendScadaDataToUIDataPoint(data);
			}
			catch (CommunicationException e)
			{
				return false;
			}
			return true;
		}

		private bool RetrySendTrees(ServerSideProxy proxy, TreeNode<NodeData> data, List<NetworkModelTreeClass> NetworkModelTreeClass)
		{
			proxy.Abort();
			proxy.Connect();
			try
			{
				proxy.Proxy.SendDataUI(data, NetworkModelTreeClass);
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

		public async Task<bool> Notify(DataToUI forcastDayAhead, int gidOfTopic)
		{
			Dictionary<string, ServerSideProxy> subscribersCopy;
			subscribersCopy = GetSubscribersCopy();

			if (forcastDayAhead == null || subscribersCopy.Count == 0)
			{
				return false;
			}

			forcastDayAhead.Topic = gidOfTopic;
			List<string> deadClients = new List<string>();

			List<string> topicSubscribers = topicSubscriptions.GetSubscribers(gidOfTopic);
			List<string> deadSubscribers = new List<string>();
			foreach (string subscriberAddress in topicSubscribers)
			{
				if (subscribersCopy.ContainsKey(subscriberAddress))
				{
					ServerSideProxy subscriberProxy = subscribersCopy[subscriberAddress];
					try
					{
						subscriberProxy.Proxy.SendScadaDataToUI(forcastDayAhead);
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

		public async Task<bool> NotifyDataPoint(List<DataPoint> data, int gidOfTopic)
        {
			Dictionary<string, ServerSideProxy> subscribersCopy;
			subscribersCopy = GetSubscribersCopy();

			if (data == null || subscribersCopy.Count == 0)
			{
				return false;
			}

			List<string> deadClients = new List<string>();


			List<string> topicSubscribers = topicSubscriptions.GetSubscribers(gidOfTopic);
			List<string> deadSubscribers = new List<string>();
			foreach (string subscriberAddress in topicSubscribers)
			{
				if (subscribersCopy.ContainsKey(subscriberAddress))
				{
					ServerSideProxy subscriber = subscribersCopy[subscriberAddress];

					try
					{
						subscriber.Proxy.SendScadaDataToUIDataPoint(data);
					}
					catch (CommunicationException ee)
					{
						if (RetrySendDataPoint(subscriber, data) == false)
						{
							deadClients.Add(subscriberAddress);
						}
						else
						{
							return true;
						}
					}
					catch (TimeoutException dd)
					{
						if (RetrySendDataPoint(subscriber, data) == false)
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

			return true;
		}

        public async Task<bool> NotifyTree(TreeNode<NodeData> data, List<NetworkModelTreeClass> NetworkModelTreeClass, int gidOfTopic)
        {
			Dictionary<string, ServerSideProxy> subscribersCopy;
			subscribersCopy = GetSubscribersCopy();

			if (data == null || NetworkModelTreeClass == null || subscribersCopy.Count == 0)
			{
				return false;
			}

			List<string> deadClients = new List<string>();

			List<string> topicSubscribers = topicSubscriptions.GetSubscribers(gidOfTopic);
			List<string> deadSubscribers = new List<string>();
			foreach (string subscriberAddress in topicSubscribers)
			{
				if (subscribersCopy.ContainsKey(subscriberAddress))
				{
					ServerSideProxy subscriber = subscribersCopy[subscriberAddress];

					try
					{
						subscriber.Proxy.SendDataUI(data, NetworkModelTreeClass);
					}
					catch (CommunicationException)
					{
						if (RetrySendTrees(subscriber, data, NetworkModelTreeClass) == false)
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
						if (RetrySendTrees(subscriber, data, NetworkModelTreeClass) == false)
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

			return true;
		}
    }
}
