using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.TransactionManager
{
    [ServiceContract]
    public interface ITransactionCheck
    {
        [OperationContract]
        Task<bool> Prepare();

        [OperationContract]
        Task Commit();

        [OperationContract]
        Task Rollback();
    }
}
