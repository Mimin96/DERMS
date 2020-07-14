using System;
using System.Collections.Generic;
using System.Text;

namespace CimProfileCreator
{
    public interface ICIMProfileCreator
    {
        string LoadCIMRDFSFile(string location);

        string GenerateDLL(string name, string version, string productName, string nameSpace);
    }
}
