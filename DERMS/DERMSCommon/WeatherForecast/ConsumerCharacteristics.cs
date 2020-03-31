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
            for (int i = 0; i < 24; i++)
            {
                TimeSpan timeSpan = new TimeSpan(i, 0, 0);
                double curve = 0.0;
                if (i == 0 || i == 15 || i == 6 || i == 10 || i == 13 || i == 23)
                {
                    curve = 0.4;
                }
                else if (i == 1 || i == 5 || i == 9 || i == 11)
                {
                    curve = 0.3;
                }
                else if (i > 1 && i < 5)
                {
                    curve = 0.2;
                }
                else if (i == 7 || i == 19 || i == 15)
                {
                    curve = 0.7;
                }
                else if (i == 8 || i == 12 || i == 22 || i == 14)
                {
                    curve = 0.5;
                }
                else if (i == 16 || i == 18 || i == 21)
                {
                    curve = 0.8;
                }
                else if (i == 20)
                {
                    curve = 0.9;
                }
                else if (i == 17)
                {
                    curve = 1;
                }



                Hourly[timeSpan] = curve;
            }
            ;
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
                    DateTime date = DateTime.Now;
                    dataPoint.Time = date.Date + HourToActivePower.Key;
                }
                if (dataPoint.Time.Hour.Equals(23))
                {
                    HourDataPoint dataPointTemp = new HourDataPoint();
                    TimeSpan timeSpan = new TimeSpan(23, 0, 0);

                    dataPointTemp.Time = dataPoint.Time.AddDays(-1).Date + timeSpan;
                    dataPointTemp.ActivePower = (float)HourToActivePower.Value;
                    dataPointTemp.ReactivePower = (float)HourToActivePower.Value / 50;

                    consumerDayAhead.Hourly.Add(dataPointTemp);
                }
                else if (dataPoint.Time.Hour.Equals(22))
                {
                    HourDataPoint dataPointTemp = new HourDataPoint();
                    TimeSpan timeSpan = new TimeSpan(22, 0, 0);

                    dataPointTemp.Time = dataPoint.Time.AddDays(-1).Date + timeSpan;
                    dataPointTemp.ActivePower = (float)HourToActivePower.Value;
                    dataPointTemp.ReactivePower = (float)HourToActivePower.Value / 50;

                    consumerDayAhead.Hourly.Add(dataPointTemp);
                }
                else
                {
                    dataPoint.ActivePower = (float)HourToActivePower.Value;
                    dataPoint.ReactivePower = (float)HourToActivePower.Value / 50;

                    consumerDayAhead.Hourly.Add(dataPoint);
                }
            }

            consumerDayAhead.Hourly = consumerDayAhead.Hourly.OrderBy(o => o.Time).ToList();
            return consumerDayAhead;
        }
    }
}
