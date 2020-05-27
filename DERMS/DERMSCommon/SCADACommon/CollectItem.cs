using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.SCADACommon
{
    [DataContract]
    [Serializable()]
    public class CollectItem
    {
        [DataMember]
        private long _gid;
        [DataMember]
        private double _p;
        [DataMember]
        private DateTime _timestamp = DateTime.Now;
        public CollectItem() { }
        public CollectItem(long gid, float p, DateTime timestamp)
        {
            Gid = gid;
            Timestamp = timestamp;
            P = p;
        }

        public long Gid { get => _gid; set => _gid = value; }
        public double P { get => _p; set => _p = value; }
        public DateTime Timestamp { get => _timestamp; set => _timestamp = value; }
    }
}



