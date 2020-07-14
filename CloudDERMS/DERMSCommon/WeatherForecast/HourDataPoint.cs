using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.WeatherForecast
{
    [DataContract]
    public class HourDataPoint
    {
        public HourDataPoint()
        {
            Time = DateTime.MinValue;
            ActivePower = 0;
            ReactivePower = 0;
        }

        [DataMember]
        public DateTime Time
        {
            get; set;
        }

        [DataMember]
        public float ActivePower
        {
            get; set;
        }

        [DataMember]
        public float ReactivePower
        {
            get; set;
        }
    }
}
