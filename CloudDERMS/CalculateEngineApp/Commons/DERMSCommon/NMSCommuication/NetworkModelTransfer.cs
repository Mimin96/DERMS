using DERMSCommon.DataModel.Core;
using FTN.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.NMSCommuication
{
    [DataContract]
    public class NetworkModelTransfer
    {
        [DataMember]
        private bool _initState;
        [DataMember]
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _insert;
        [DataMember]
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _update;
        [DataMember]
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _delete;

        public NetworkModelTransfer(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> insert,
                                    Dictionary<DMSType, Dictionary<long, IdentifiedObject>> update,
                                    Dictionary<DMSType, Dictionary<long, IdentifiedObject>> delete) 
        {
            _initState = false;
            _insert = insert;
            _update = update;
            _delete = delete;
        }

        public bool InitState 
        {
            get 
            {
                return _initState;
            }
            set 
            {
                _initState = value;
            }
        }

        public Dictionary<DMSType, Dictionary<long, IdentifiedObject>> Insert
        {
            get
            {
                return _insert;
            }
        }
        public Dictionary<DMSType, Dictionary<long, IdentifiedObject>> Update
        {
            get
            {
                return _update;
            }
        }
        public Dictionary<DMSType, Dictionary<long, IdentifiedObject>> Delete
        {
            get
            {
                return _delete;
            }
        }
    }
}
