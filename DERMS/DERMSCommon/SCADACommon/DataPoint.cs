using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace DERMSCommon.SCADACommon
{
    [DataContract]
    [Serializable()]
    public class DataPoint
    {
        [DataMember]
        private PointType _type;
        [DataMember]
        private ushort _address;
        [DataMember]
        private DateTime _timestamp = DateTime.Now;
        [DataMember]
        private string _name = string.Empty;
        [DataMember]
        private long _gid;
        [DataMember]
        private string _value;
        [DataMember]
        private ushort _rawValue;
        [DataMember]
        private PackIconKind _alarmImage;
        [DataMember]
        private SolidColorBrush _alarmImageColor;
        [DataMember]
        protected AlarmType _alarm;

        public DataPoint(long gid, PointType type, ushort address, DateTime timestamp, string name, string value, ushort rawValue, AlarmType alarm)
        {
            _gid = gid;
            _type = type;
            _address = address;
            _timestamp = timestamp;
            _name = name;
            _value = value;
            _rawValue = rawValue;
            _alarm = alarm;

        }

        public PointType Type
        {
            get
            {
                return _type;
            }
        }

        public ushort Address
        {
            get
            {
                return _address;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public long Gid
        {
            get
            {
                return _gid;
            }
        }

        public ushort RawValue
        {
            get
            {
                return _rawValue;
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
        }

        public AlarmType Alarm
        {
            get
            {
                return _alarm;
            }
            set
            {
                _alarm = value;
            }
        }

        public PackIconKind AlarmImage
        {
            get
            {
                return _alarmImage;
            }
            set
            {
                _alarmImage = value;
            }
        }

        public SolidColorBrush AlarmImageColor
        {
            get
            {
                return _alarmImageColor;
            }
            set
            {
                _alarmImageColor = value;
            }
        }

    }
}
