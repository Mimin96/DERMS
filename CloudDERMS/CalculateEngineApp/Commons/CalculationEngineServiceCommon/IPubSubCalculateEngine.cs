using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface IPubSubCalculateEngine
    {
        [OperationContract]
        void Subscribe(string clientAddress, int gidOfTopic);

        [OperationContract]
        void Unsubscribe(string clientAddress, int gidOfTopic, bool disconnect);
    }
}
