using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon.WeatherForecast
{
    public class ConsumerCharacteristics
    {
        public ConsumerCharacteristics()
        {
            Hourly = new Dictionary<TimeSpan, double>();
            TimeSpan timeSpan = new TimeSpan(3, 0, 0);
            double typicalConsumption =24.3;
            Hourly[timeSpan] = typicalConsumption;
        }

        public Dictionary<TimeSpan, double> Hourly
        {
            get;
            private set;
        }

        public DayAhead GetDayAhead()
        {
            DayAhead consumerDayAhead = new DayAhead();
            foreach (KeyValuePair<TimeSpan, double> HourToActivePower in Hourly)
            {
                HourDataPoint dataPoint = new HourDataPoint();
                if (HourToActivePower.Key.Hours >= DateTime.Now.Hour || HourToActivePower.Key.Hours == 0)
                {
                    dataPoint.Time = DateTime.Now.Date + HourToActivePower.Key;
                }
                else
                {
                    DateTime date = DateTime.Now.AddDays(1);
                    dataPoint.Time = date.Date + HourToActivePower.Key;
                }

                dataPoint.ActivePower = (float)HourToActivePower.Value;
                dataPoint.ReactivePower = (float)HourToActivePower.Value / 50;

                consumerDayAhead.Hourly.Add(dataPoint);
            }

            consumerDayAhead.Hourly = consumerDayAhead.Hourly.OrderBy(o => o.Time).ToList();
            return consumerDayAhead;
        }
    }
}
