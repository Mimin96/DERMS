using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface ISendEventsToUI
    {
        [OperationContract]
        void ReceiveEventsFromCE(string rec);
    }
}
