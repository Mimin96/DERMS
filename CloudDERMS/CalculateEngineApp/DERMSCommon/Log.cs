using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
    public class Log
    {
        private string _message;
        private Enums.Component _component;
        private Enums.LogLevel _logLevel;
        private DateTime _dateTime;

        public Log(string message, Enums.Component component, Enums.LogLevel logLevel, DateTime dateTime)
        {
            Message = message;
            Component = component;
            LogLevel = logLevel;
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
        public Enums.LogLevel LogLevel
        {
            get
            {
                return _logLevel;
            }
            set
            {
                _logLevel = value;
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
