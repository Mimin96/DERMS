using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface IUpdateCommand
    {
        [OperationContract]
        void UpdateCommandScada(DataFromScadaToCE data);
    }
}
