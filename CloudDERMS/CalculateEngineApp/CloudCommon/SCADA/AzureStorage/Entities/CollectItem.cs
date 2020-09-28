﻿using Microsoft.Azure.Cosmos.Table;
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
        [DataMember]
        private string _date;
        [DataMember]
        private DateTime _timestamp;

        public CollectItem() { }
        public CollectItem(long gid, float p, DateTime timestamp)
        {
            DateTime = timestamp;
            Timestamp = timestamp;
            PartitionKey = "CollectItem";
            RowKey = gid.ToString() + " " + timestamp.ToString("o");

            Date = timestamp.Date.Year + "-" + timestamp.Date.Month + "-" + timestamp.Date.Day;
            Gid = gid;
            Timestamp = timestamp;
            P = p;
        }

        public string Date { get => _date; set => _date = value; }
        public long Gid { get => _gid; set => _gid = value; }
        public double P { get => _p; set => _p = value; }
        public DateTime DateTime { get => _timestamp; set => _timestamp = value; }
    }
}



