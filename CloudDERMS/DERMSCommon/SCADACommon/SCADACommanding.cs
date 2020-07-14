using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    [DataContract]
    public class SCADACommanding
    {
        [DataMember]
        private long _gid;
        [DataMember]
        private ushort _commandedValue;
        [DataMember]
        private PointType _type;

        public SCADACommanding(long gid, ushort commandedValue, PointType type) 
        {
            _gid = gid;
            _commandedValue = commandedValue;
            _type = type;
        }

        public long Gid
        {
            get
            {
                return _gid;
            }
        }

        public PointType Type
        {
            get
            {
                return _type;
            }
        }

        public ushort CommandedValue
        {
            get
            {
                return _commandedValue;
            }
            set
            {
                _commandedValue = value;
            }
        }
    }
}
