using DERMSCommon;
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
    public class EventStorage : TableEntity
    {
        [DataMember]
        private string _message;
        [DataMember]
        private Enums.Component _component;
        //[DataMember]
        // private DateTime _dateTime;

        public EventStorage()
        {

        }

        public EventStorage(string message, Enums.Component component, DateTime dateTime)
        {
            Timestamp = dateTime;
            PartitionKey = "EventStorage";
            RowKey = component.ToString() + " " + dateTime.ToString("o");

            Message = message;
            Component = component;
            //DateTime = dateTime;
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        public Enums.Component Component
        {
            get
            {
                return _component;
            }
            set
            {
                _component = value;
            }
        }
        //public DateTime DateTime
        //{
        //    get
        //    {
        //        return _dateTime;
        //    }
        //    set
        //    {
        //        _dateTime = value;
        //    }
        //}
    }
}
