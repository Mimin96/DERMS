using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.WeatherForecast
{
    [DataContract]
    public class DerForecastDayAhead
    {
        public DerForecastDayAhead()
        {
            Consumption = new DayAhead();
            Production = new DayAhead();
        }

        public DerForecastDayAhead(long entityGid)
        {
            this.entityGid = entityGid;
            Consumption = new DayAhead();
            Production = new DayAhead();
        }

		public DerForecastDayAhead(DerForecastDayAhead entity)
		{
			Production = CloneHourly(entity.Production);
			Consumption = CloneHourly(entity.Consumption);
			entityGid = entity.entityGid;
		}

		public DayAhead CloneHourly(DayAhead dayAhead)
		{
			DayAhead result = new DayAhead();

			if (!dayAhead.Hourly.Count.Equals(0))
			{
				for (int i = 0; i < 24; i++)
				{
					HourDataPoint dataPoint = new HourDataPoint();
					dataPoint.ActivePower = dayAhead.Hourly[i].ActivePower;
					dataPoint.ReactivePower = dayAhead.Hourly[i].ReactivePower;
					dataPoint.Time = dayAhead.Hourly[i].Time;
					result.Hourly.Add(dataPoint);
				}
			}

			return result;
		}

		[DataMember]
        public long entityGid
        {
            get; set;
        }

        [DataMember]
        public DayAhead Production
        {
            get; set;
        }

        [DataMember]
        public DayAhead Consumption
        {
            get; set;
        }
    }
}
