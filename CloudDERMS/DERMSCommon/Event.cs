using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
    [Serializable()]
    public class Event
    {
        private string _message;
        private Enums.Component _component;
        private DateTime _dateTime;

        public Event()
        {
            
        }

        public Event(string message, Enums.Component component, DateTime dateTime)
        {
            Message = message;
            Component = component;
            DateTime = dateTime;
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
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
            }
        }
    }
}
