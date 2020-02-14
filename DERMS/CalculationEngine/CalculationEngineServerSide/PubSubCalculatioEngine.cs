using CalculationEngineServiceCommon;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CalculationEngineService
{
    public class PubSubCalculatioEngine : IPubSubCalculateEngine, INotify
    {
        private Dictionary<string, ServerSideProxy> subscribers = new Dictionary<string, ServerSideProxy>();
        private TopicSubscriptions topicSubscriptions = new TopicSubscriptions();
        private object subscribersLock = new object();

        private static PubSubCalculatioEngine instance = null;

        public static PubSubCalculatioEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PubSubCalculatioEngine();
                }

                return instance;

            }
        }

        public void Notify(Dictionary<long, DerForecastDayAhead> forcastDayAhead, int gidOfTopic)
        {
            if (forcastDayAhead == null || subscribers.Count == 0)
            {
                return;
            }

            Dictionary<string, ServerSideProxy> subscribersCopy;
            List<string> deadClients = new List<string>();
            lock (subscribersLock)
            {
                subscribersCopy = new Dictionary<string, ServerSideProxy>(subscribers);
            }

            List<string> topicSubscribers = topicSubscriptions.GetSubscribers(gidOfTopic);
            List<string> deadSubscribers = new List<string>();
            foreach (string subscriberAddress in topicSubscribers)
            {
                if (subscribersCopy.ContainsKey(subscriberAddress))
                {
                    ServerSideProxy subscriber = subscribersCopy[subscriberAddress];
                    try
                    {
                        subscriber.Proxy.SendDataUI(forcastDayAhead);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    deadSubscribers.Add(subscriberAddress);
                }

                topicSubscriptions.RemoveDeadSubscribers(gidOfTopic, deadSubscribers);
            }

            RemoveDeadClients(deadClients);
        }

        public void Subscribe(string clientAddress, int gidOfTopic)
        {
            lock (subscribersLock)
            {
                if (!subscribers.ContainsKey(clientAddress))
                {
                    subscribers.Add(clientAddress, new ServerSideProxy(clientAddress));
                }
                topicSubscriptions.Subscribe(clientAddress, gidOfTopic);
            }
        }

        public void Unsubscribe(string clientAddress, int gidOfTopic, bool disconnect)
        {
            lock (subscribersLock)
            {
                if (disconnect)
                {
                    //close connection
                    subscribers.Remove(clientAddress);
                }
                topicSubscriptions.Unsubscribe(clientAddress, gidOfTopic);
            }
        }

        private void RemoveDeadClients(List<string> deadClients)
        {
            if (deadClients.Count > 0)
            {
                lock (subscribersLock)
                {
                    foreach (string clientAddress in deadClients)
                    {
                        subscribers.Remove(clientAddress);
                    }
                }
            }
        }
    }
}
