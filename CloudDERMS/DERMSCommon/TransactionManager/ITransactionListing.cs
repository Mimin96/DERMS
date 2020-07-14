using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.TransactionManager
{
    [ServiceContract]
    public interface ITransactionListing
    {
        [OperationContract]
        void Enlist(string adress);

        [OperationContract]
        void FinishList(bool IsSuccessfull);
    }
}
