using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.SCADA
{
    [ServiceContract]
    public interface IEvetnsDatabase
    {
        [OperationContract]
        Task<List<Event>> GetEvents();
        [OperationContract]
        Task SetEvent(Event eventt);
    }
}
