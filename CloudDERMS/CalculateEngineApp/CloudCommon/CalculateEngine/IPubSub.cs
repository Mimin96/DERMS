using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
	[ServiceContract]
	public interface IPubSub
	{
		[OperationContract]
		Task<bool> SubscribeSubscriber(string clientAddress, int gidOfTopic);
	}
}
