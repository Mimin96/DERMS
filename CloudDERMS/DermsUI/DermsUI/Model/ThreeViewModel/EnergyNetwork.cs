using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model.ThreeViewModel
{
    public class EnergyNetwork : INotifyPropertyChanged
    {
        private List<GeographicalRegion> geographicalRegion;
        private long gID;
        public EnergyNetwork()
        {
            EnergyNetworkName = "EnergyNetwork";
            geographicalRegion = new List<GeographicalRegion>()
            {
                new GeographicalRegion("GeographicalRegion 1"){ GID = 1 },
                new GeographicalRegion("GeographicalRegion 2"){ GID = 2 }
            };
        }
        public List<GeographicalRegion> GeographicalRegion
        {
            get
            {
                return geographicalRegion;
            }
            set
            {
                geographicalRegion = value;
                OnPropertyChanged("GeographicalRegion");
            }
        }

        public long GID
        {
            get { return gID; }
            set { gID = value; OnPropertyChanged("GID"); }
        }

        public string EnergyNetworkName
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
