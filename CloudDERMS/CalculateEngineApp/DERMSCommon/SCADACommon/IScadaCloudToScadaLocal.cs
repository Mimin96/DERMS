using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    public interface IScadaCloudToScadaLocal
    {
        Task <bool> SendEndpoints(string endpoint);
    }
}
