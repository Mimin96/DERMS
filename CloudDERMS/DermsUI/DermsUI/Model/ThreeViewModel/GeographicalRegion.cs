using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model.ThreeViewModel
{
    public class GeographicalRegion : INotifyPropertyChanged
    {
        private List<Substation> substations;
        private long gID;
        public GeographicalRegion(string name)
        {
            GeographicalRegionName = name;
            substations = new List<Substation>()
            {
                new Substation("Substation 1"){ GID = 3 },
                new Substation("Substation 2"){ GID = 4 }
            };
        }
        public List<Substation> Substations
        {
            get
            {
                return substations;
            }
            set
            {
                substations = value;
                OnPropertyChanged("Substations");
            }
        }
        public string GeographicalRegionName
        {
            get;
            set;
        }

        public long GID
        {
            get { return gID; }
            set { gID = value; OnPropertyChanged("GID"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
