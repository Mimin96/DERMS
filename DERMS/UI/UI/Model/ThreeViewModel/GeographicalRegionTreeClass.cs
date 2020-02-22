using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources;

namespace UI.Model.ThreeViewModel
{
    public class GeographicalRegionTreeClass : BindableBase
    {
        private List<GeographicalSubRegionTreeClass> _geographicalSubRegions;
        private string _name;
        private long _gID;
        private string _type; // ovo promenuti u enum kada se spoji sa sotatkom koda

        public GeographicalRegionTreeClass(string name, long gid, string type)
        {
            _geographicalSubRegions = new List<GeographicalSubRegionTreeClass>();
            _name = name;
            _gID = gid;
            _type = type;
        }

        public List<GeographicalSubRegionTreeClass> GeographicalSubRegions
        {
            get { return _geographicalSubRegions; }
            set { _geographicalSubRegions = value; OnPropertyChanged("GeographicalSubRegions"); }
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
