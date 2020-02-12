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
