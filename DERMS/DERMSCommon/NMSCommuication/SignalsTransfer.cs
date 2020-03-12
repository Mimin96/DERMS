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
    public class SignalsTransfer
    {
        [DataMember]
        private Dictionary<int, Dictionary<long, IdentifiedObject>> _insert;
        [DataMember]
        private Dictionary<int, Dictionary<long, IdentifiedObject>> _update;
        [DataMember]
        private Dictionary<int, Dictionary<long, IdentifiedObject>> _delete;

        public SignalsTransfer(Dictionary<int, Dictionary<long, IdentifiedObject>> insert,
                                    Dictionary<int, Dictionary<long, IdentifiedObject>> update,
                                    Dictionary<int, Dictionary<long, IdentifiedObject>> delete)
        {
            _insert = insert;
            _update = update;
            _delete = delete;
        }

        public Dictionary<int, Dictionary<long, IdentifiedObject>> Insert
        {
            get
            {
                return _insert;
            }
        }
        public Dictionary<int, Dictionary<long, IdentifiedObject>> Update
        {
            get
            {
                return _update;
            }
        }
        public Dictionary<int, Dictionary<long, IdentifiedObject>> Delete
        {
            get
            {
                return _delete;
            }
        }
    }
}
