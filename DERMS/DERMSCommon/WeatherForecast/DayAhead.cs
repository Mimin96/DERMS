﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.WeatherForecast
{
    [DataContract]
    public class DayAhead
    {
        public DayAhead()
        {
            Hourly = new List<HourDataPoint>(24);
        }

        [DataMember]
        public List<HourDataPoint> Hourly
        {
            get;
            set;
        }

        public static DayAhead operator +(DayAhead c1, DayAhead c2)
        {
            DayAhead result = new DayAhead();
            if (c1.Hourly.Count == 0)
            {
                return c2;
            }
            else
            {
                for (int i = 0; i < 24; i++)
                {
                    HourDataPoint dataPoint = new HourDataPoint();
                    dataPoint.ActivePower = c1.Hourly[i].ActivePower + c2.Hourly[i].ActivePower;
                    dataPoint.ReactivePower = c1.Hourly[i].ReactivePower + c2.Hourly[i].ReactivePower;
                    dataPoint.Time = c1.Hourly[i].Time;
                    result.Hourly.Add(dataPoint);
                }
            }
            return result;
        }
    }
}
