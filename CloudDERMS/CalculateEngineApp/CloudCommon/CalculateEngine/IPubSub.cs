using DERMSCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
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
		Task<bool> Notify(DataToUI forcastDayAhead, int gidOfTopic);
		[OperationContract]
		Task<bool> NotifyDataPoint(List<DataPoint> data, int gidOfTopic);
		[OperationContract]
		Task<bool> NotifyTree(TreeNode<NodeData> data, List<NetworkModelTreeClass> NetworkModelTreeClass, int gidOfTopic);
		[OperationContract]
		Task<bool> NotifyEvents(Event @event, int gidOfTopic);
		[OperationContract]
		Task<bool> SubscribeSubscriber(string clientAddress, int gidOfTopic);
		[OperationContract]
		Task<bool> SubscribeOnMultipleTopics(string clientAddress, List<int> gidOfTopic);
	}
}
