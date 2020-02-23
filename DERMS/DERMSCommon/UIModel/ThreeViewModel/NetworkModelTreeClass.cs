﻿using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTN.Common;
using System.Runtime.Serialization;

namespace DERMSCommon.UIModel.ThreeViewModel
{
    [DataContract]
    public class NetworkModelTreeClass : BindableBase
    {
        [DataMember]
        private List<GeographicalRegionTreeClass> _geographicalRegions;
        [DataMember]
        private string _name;
        [DataMember]
        private long _gID;
        [DataMember]
        private DMSType _type;

        public NetworkModelTreeClass(string name, long gid, DMSType type)
        {
            _geographicalRegions = new List<GeographicalRegionTreeClass>();
            _name = name;
            _gID = gid;
            _type = type;
        }

        public List<GeographicalRegionTreeClass> GeographicalRegions
        {
            get { return _geographicalRegions; }
            set { _geographicalRegions = value; OnPropertyChanged("GeographicalRegions"); }
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
    }
}