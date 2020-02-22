using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources;

namespace UI.Model.ThreeViewModel
{
    public class SubstationTreeClass : BindableBase
    {
        private List<SubstationElementTreeClass> _substationElements;
        private string _name;
        private long _gID;
        private string _type; // ovo promenuti u enum kada se spoji sa sotatkom koda

        public SubstationTreeClass(string name, long gid, string type)
        {
            _substationElements = new List<SubstationElementTreeClass>();
            _name = name;
            _gID = gid;
            _type = type;
        }

        public List<SubstationElementTreeClass> SubstationElements
        {
            get { return _substationElements; }
            set { _substationElements = value; OnPropertyChanged("SubstationElements"); }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        public long GID
        {
            get { return _gID; }
            set { _gID = value; OnPropertyChanged("GID"); }
        }
        public string Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }
    }
}
