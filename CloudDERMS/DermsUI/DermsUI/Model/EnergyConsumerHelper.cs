using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model
{
    public class EnergyConsumerHelper
    {
        private bool privacyState;
        private string customType;
        private string description;
        private int consumerCount;
        //private double pFixed;
        //private double qFixed;

        public EnergyConsumerHelper()
        {

        }

        public bool PrivacyState { get { return privacyState; } set { privacyState = value;  } }
        public string CustomType { get { return customType; } set { customType = value;  } }
        public string Description { get { return description; } set { description = value;  } }
        public int ConsumerCount { get { return consumerCount; } set { consumerCount = value;  } }
        //public double PFixed { get { return pFixed; } set { pFixed = value;  } }
        //public double QFixed { get { return qFixed; } set { qFixed = value;  } }

    }
}
