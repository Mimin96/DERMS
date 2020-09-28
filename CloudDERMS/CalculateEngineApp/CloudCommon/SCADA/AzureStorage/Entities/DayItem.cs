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
    public class DayItem : TableEntity
    {
        [DataMember]
        private long _gid;
        [DataMember]
        private double _pMin;
        [DataMember]
        private double _pMax;
        [DataMember]
        private double _pAvg;
        [DataMember]
        private double _e;
        [DataMember]
        private double _p;
        [DataMember]
        private string _date;
         [DataMember]
         private DateTime _timestamp;
        public DayItem() { }
        public DayItem(long gid, DateTime timestamp, double pMin, double pMax, double pAvg, double e, double p)
        {
            Timestamp = timestamp;
            PartitionKey = "DayItem";
            RowKey = gid.ToString() + " " + timestamp.ToString("o");

            Date = timestamp.Date.Year + "-" + timestamp.Date.Month;
            Gid = gid;
            DateTime = timestamp;
            PMin = pMin;
            PMax = pMax;
            PAvg = pAvg;
            E = e;
            P = p;
        }

        public string Date { get => _date; set => _date = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public double PMin { get => _pMin; set => _pMin = value; }
        public double PAvg { get => _pAvg; set => _pAvg = value; }
        public double PMax { get => _pMax; set => _pMax = value; }
        public double E { get => _e; set => _e = value; }
        public DateTime DateTime { get => _timestamp; set => _timestamp = value; }
        public double P { get => _p; set => _p = value; }
    }
}
