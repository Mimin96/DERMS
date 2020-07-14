using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model
{
    public class ConnectivityNodeHelper
    {
        private string description;

        public ConnectivityNodeHelper()
        {
            description = "";
        }

        public string Description { get { return description; } set { description = value; } }

    }
}
