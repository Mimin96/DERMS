﻿using DERMSCommon.UIModel;
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

        public SubstationElementTreeClass(string name, long gid, DMSType type, float p)
        {
            _name = name;
            _gID = gid;
            _type = type;
            _p = p;
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
    }
}
