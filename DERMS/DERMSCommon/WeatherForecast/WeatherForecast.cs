using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.WeatherForecast
{
    [DataContract]
    public class WeatherForecast
    {
        [DataMember]
        private long _gid;
        [DataMember]
        private float _windSpeed;
        [DataMember]
        private float _visibility;
        [DataMember]
        private float _temperature;
        [DataMember]
        private DateTime _time;
        [DataMember]
        private string _summary;
        [DataMember]
        private float _cloudCover;

        public WeatherForecast(long gid, float windSpeed, float visibility, float temperature, float cloudCover, DateTime time, string summary)
        {
            _gid = gid;
            _windSpeed = windSpeed;
            _visibility = visibility;
            _temperature = temperature;
            _time = time;
            _cloudCover = cloudCover;
            _summary = summary;
        }

        public long Gid { get { return _gid; } }
        public float WindSpeed { get { return _windSpeed; } }
        public float Visibility { get { return _visibility; } }
        public float Temperature { get { return _temperature; } }
        public float CloudCover { get { return _cloudCover; } }
        public DateTime Time { get { return _time; } }
        public string Summary { get { return _summary; } }
    }
}
