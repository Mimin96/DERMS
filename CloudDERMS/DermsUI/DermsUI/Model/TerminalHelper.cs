using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model
{
    public class TerminalHelper
    {
        private string description;
        private bool connected;
        private int seqNumber;


        public TerminalHelper()
        {

        }

        public string Description { get { return description; } set { description = value; } }
        public bool Connected { get { return connected; } set { connected = value; } }
        public int SeqNumber { get { return seqNumber; } set { seqNumber = value; } }


    }
}
