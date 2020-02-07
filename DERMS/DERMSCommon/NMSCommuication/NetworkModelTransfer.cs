using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
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
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _insert;
        [DataMember]
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _update;
        [DataMember]
        private Dictionary<DMSType, Dictionary<long, IdentifiedObject>> _delete;

        public NetworkModelTransfer(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> insert,
                                    Dictionary<DMSType, Dictionary<long, IdentifiedObject>> update,
                                    Dictionary<DMSType, Dictionary<long, IdentifiedObject>> delete) 
        {
            _insert = insert;
            _update = update;
            _delete = delete;
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
