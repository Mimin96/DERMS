using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.Model.ThreeViewModel
{
    public class Feeder : INotifyPropertyChanged
    {
        private string feederName = string.Empty;
        private long gID;
        public Feeder(string name)
        {
            FeederName = name;
        }

        public string FeederName
        {
            get
            {
                return feederName;
            }
            set
            {
                feederName = value;
                OnPropertyChanged("FeederName");
            }
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
