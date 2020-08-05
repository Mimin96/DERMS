using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.ESI.SIMES.CIM.CIMProfileCreator
{
    public interface ICimProfileCreator
    {
        string LoadCIMRDFSFile(string location);

        string GenerateDLL(string name, string version, string productName, string nameSpace);
    }
}
