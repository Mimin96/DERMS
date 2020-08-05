using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService
{
    [ServiceContract]
    public interface INetworkModelDeepCopy
    {
        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
        
    }
}
