using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.SCADA.AzureStorage.Entities
{
    [DataContract]
    [Serializable()]
    public class CollectItem : TableEntity
    {
        [DataMember]
        private long _gid;
        [DataMember]
        private double _p;
        //[DataMember]
        //private DateTime _timestamp = DateTime.Now;

        public CollectItem() { }
        public CollectItem(long gid, float p, DateTime timestamp)
        {
            Timestamp = timestamp;
            PartitionKey = "CollectItem";
            RowKey = gid.ToString() + " " + timestamp.ToString("o");

            Gid = gid;
            Timestamp = timestamp;
            P = p;
        }

        public long Gid { get => _gid; set => _gid = value; }
        public double P { get => _p; set => _p = value; }
        //public DateTime Timestamp { get => _timestamp; set => _timestamp = value; }
    }
}



