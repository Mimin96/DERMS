using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    class TopicSubscriptions
    {
        private Dictionary<long, List<string>> topicSubscriptions = new Dictionary<long, List<string>>();
        private static object topicSubscriptionsLock = new object();

        public void Subscribe(string clientAddress, long gidOfTopic)
        {
            lock (topicSubscriptionsLock)
            {
                if (topicSubscriptions.ContainsKey(gidOfTopic))
                {
                    topicSubscriptions[gidOfTopic].Add(clientAddress);
                }
                else
                {
                    topicSubscriptions.Add(gidOfTopic, new List<string>());
                    topicSubscriptions[gidOfTopic].Add(clientAddress);
                }
            }
        }

        public void Unsubscribe(string clientAddress, long gidOfTopic)
        {
            lock (topicSubscriptionsLock)
            {
                if (topicSubscriptions.ContainsKey(gidOfTopic))
                {
                    int index = topicSubscriptions[gidOfTopic].IndexOf(clientAddress);
                    if (index != -1)
                    {
                        topicSubscriptions[gidOfTopic].RemoveAt(index);
                    }
                }
            }
        }

        public List<long> GetTopics()
        {
            lock (topicSubscriptionsLock)
            {
                return topicSubscriptions.Keys.ToList();
            }
        }

        public List<string> GetSubscribers(long gidOfTopic)
        {
            lock (topicSubscriptionsLock)
            {
                if (topicSubscriptions.ContainsKey(gidOfTopic))
                {
                    return new List<string>(topicSubscriptions[gidOfTopic]);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public void RemoveDeadSubscribers(long gidOfTopic, List<string> deadSubscribers)
        {
            lock (topicSubscriptionsLock)
            {
                foreach (string deadSubscriber in deadSubscribers)
                {
                    topicSubscriptions[gidOfTopic].Remove(deadSubscriber);
                }
            }
        }
    }
}
