using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model.ThreeViewModel
{
    public class Substation : INotifyPropertyChanged
    {
        private List<Feeder> feeders;
        private long gID;
        public Substation(string name)
        {
            SubstationName = name;
            feeders = new List<Feeder>()
            {
                new Feeder("Feeder 1"){ GID = 5 },
                new Feeder("Feeder 2"){ GID = 6 }
            };
        }
        public List<Feeder> Feeders
        {
            get
            {
                return feeders;
            }
            set
            {
                feeders = value;
                OnPropertyChanged("Feeders");
            }
        }
        public string SubstationName
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
