using CloudCommon.CalculateEngine;
using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CEPubSubMicroservice
{
	[DataContract]
	public class ServerSideProxy
	{
		[IgnoreDataMember]
		public ChannelFactory<ICECommunicationPubSub> factory { get; set; }
		[IgnoreDataMember]
		public ICECommunicationPubSub Proxy { get; set; }
		[DataMember]
		public string ClientAddress { get; set; }

		public ServerSideProxy() { }

		public ServerSideProxy(string clientAddress)
		{
			this.ClientAddress = clientAddress;
			Connect();
			//SendInitialDerForecastDayAhead();
		}

		public void Connect()
		{
			NetTcpBinding binding = new NetTcpBinding();
			factory = new ChannelFactory<ICECommunicationPubSub>(binding, new EndpointAddress(ClientAddress));
			Proxy = factory.CreateChannel();
		}

		public void SendInitialDerForecastDayAhead()
		{
			DataToUI data = new DataToUI();
			try
			{
				Proxy.SendDataToUI(data);
			}
			catch (CommunicationException e)
			{
				Abort();
				Connect();
			}
			catch (TimeoutException)
			{
				Abort();
				Connect();
			}
		}

		public void Abort()
		{
			factory.Abort();
		}

		public void Close()
		{
			factory.Close();
		}
	}
}
