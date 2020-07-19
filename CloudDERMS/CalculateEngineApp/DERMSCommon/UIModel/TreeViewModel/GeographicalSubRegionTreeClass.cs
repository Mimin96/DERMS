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
    public class GeographicalSubRegionTreeClass : BindableBase
    {
        [DataMember]
        private List<SubstationTreeClass> _substations;
        [DataMember]
        private string _name;
        [DataMember]
        private long _gID;
        [DataMember]
        private DMSType _type;
		[DataMember]
		private float _minFlexibility;
		[DataMember]
		private float _maxFlexibility;

		public GeographicalSubRegionTreeClass(string name, long gid, DMSType type, float min, float max)
        {
            _substations = new List<SubstationTreeClass>();
            _name = name;
            _gID = gid;
            _type = type;
			_minFlexibility = min;
			_maxFlexibility = max;
		}

        public List<SubstationTreeClass> Substations
        {
            get { return _substations; }
            set { _substations = value; OnPropertyChanged("Substations"); }
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
