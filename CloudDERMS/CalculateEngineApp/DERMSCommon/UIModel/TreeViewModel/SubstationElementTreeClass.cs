using DERMSCommon.UIModel;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.UIModel.ThreeViewModel
{
    [DataContract]
    public class SubstationElementTreeClass : BindableBase
    {
        [DataMember]
        private string _name;
        [DataMember]
        private long _gID;
        [DataMember]
        private DMSType _type;
        [DataMember]
        private float _p;
		[DataMember]
		private float _minFlexibility;
		[DataMember]
		private float _maxFlexibility;

		public SubstationElementTreeClass(string name, long gid, DMSType type, float p, float min, float max)
        {
            _name = name;
            _gID = gid;
            _type = type;
            _p = p;
			_minFlexibility = min;
			_maxFlexibility = max;
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
        public DMSType Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }
        public float P
        {
            get { return _p; }
            set { _p = value; OnPropertyChanged("P"); }
        }
		public float MinFlexibility
		{
			get { return _minFlexibility; }
			set { _minFlexibility = value; OnPropertyChanged("MinFlexibility"); }
		}
		public float MaxFlexibility
		{
			get { return _maxFlexibility; }
			set { _maxFlexibility = value; OnPropertyChanged("MaxFlexibility"); }
		}
	}
}
