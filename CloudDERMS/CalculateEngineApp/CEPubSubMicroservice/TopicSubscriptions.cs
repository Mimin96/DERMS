using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEPubSubMicroservice
{
	public class TopicSubscriptions
	{
		private IReliableDictionary<long, List<string>> topicSubscriptions;
		private readonly IReliableStateManager stateManager;

		public TopicSubscriptions(IReliableStateManager StateManager)
		{
			this.stateManager = StateManager;
		}

		public async Task SubscribeAsync(string clientAddress, long gidOfTopic)
		{
			using (var tx = this.stateManager.CreateTransaction())
			{
				topicSubscriptions = await stateManager.GetOrAddAsync<IReliableDictionary<long, List<string>>>("topicSubscriptions");
				ConditionalValue<List<string>> clientAddresses = await topicSubscriptions.TryGetValueAsync(tx, gidOfTopic);
				if (clientAddresses.HasValue)
				{
					clientAddresses.Value.Add(clientAddress);
					await topicSubscriptions.AddOrUpdateAsync(tx, gidOfTopic, clientAddresses.Value, (key, value) => value = clientAddresses.Value);
				}
				else
				{
					List<string> clientAdresses = new List<string>() { clientAddress };
					await topicSubscriptions.AddOrUpdateAsync(tx, gidOfTopic, clientAdresses, (key, value) => value = clientAdresses);
				}

				await tx.CommitAsync();
			}
		}

		public async void Unsubscribe(string clientAddress, long gidOfTopic)
		{
			using (var tx = this.stateManager.CreateTransaction())
			{
				topicSubscriptions = await stateManager.GetOrAddAsync<IReliableDictionary<long, List<string>>>("topicSubscriptions");
				ConditionalValue<List<string>> clientAddresses = await topicSubscriptions.TryGetValueAsync(tx, gidOfTopic);
				if (clientAddresses.HasValue)
				{
					int index = clientAddresses.Value.IndexOf(clientAddress);
					if (index != -1)
					{
						clientAddresses.Value.RemoveAt(index);
					}
					await topicSubscriptions.AddOrUpdateAsync(tx, gidOfTopic, clientAddresses.Value, (key, value) => value = clientAddresses.Value);
				}

				await tx.CommitAsync();
			}
		}

		public List<string> GetSubscribers(long gidOfTopic)
		{
			using (var tx = this.stateManager.CreateTransaction())
			{
				topicSubscriptions = stateManager.GetOrAddAsync<IReliableDictionary<long, List<string>>>("topicSubscriptions").Result;
				ConditionalValue<List<string>> clientAddresses = topicSubscriptions.TryGetValueAsync(tx, gidOfTopic).Result;
				if (clientAddresses.HasValue)
				{
					return clientAddresses.Value.ToList();
				}
				else
				{
					return new List<string>();
				}
			}
		}

		public async Task RemoveDeadSubscribersForTopicAsync(long gidOfTopic, List<string> deadSubscribers)
		{
			if (deadSubscribers.Count == 0)
			{
				return;
			}

			using (var tx = this.stateManager.CreateTransaction())
			{
				topicSubscriptions = stateManager.GetOrAddAsync<IReliableDictionary<long, List<string>>>("topicSubscriptions").Result;
				ConditionalValue<List<string>> clientAddresses = topicSubscriptions.TryGetValueAsync(tx, gidOfTopic).Result;
				if (clientAddresses.HasValue)
				{
					foreach (string clientAddress in deadSubscribers)
					{
						clientAddresses.Value.Remove(clientAddress);
					}
				}
				await topicSubscriptions.AddOrUpdateAsync(tx, gidOfTopic, clientAddresses.Value, (key, value) => value = clientAddresses.Value);
				await tx.CommitAsync();
			}
		}
	}
}
