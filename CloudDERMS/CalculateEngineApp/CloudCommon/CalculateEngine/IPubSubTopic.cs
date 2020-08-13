using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
	[ServiceContract]
	public interface IPubSubTopic
	{
		[OperationContract]
		Task<bool> SubscribeTopic(string clientAddress, long gidOfTopic);
	}
}
