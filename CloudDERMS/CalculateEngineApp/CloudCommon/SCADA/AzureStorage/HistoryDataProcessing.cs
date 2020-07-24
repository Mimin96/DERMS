using CloudCommon.SCADA.AzureStorage.Entities;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.SCADA.AzureStorage
{
    public class HistoryDataProcessing
    {
        public Dictionary<Tuple<long, DateTime>, CollectItem> ConvertDataPoints(List<DataPoint> pointTypeToConfiguration)
        {
            Dictionary<Tuple<long, DateTime>, CollectItem> collectItems = new Dictionary<Tuple<long, DateTime>, CollectItem>();
            Tuple<long, DateTime> key;
            CollectItem item;

            foreach (var dataPoint in pointTypeToConfiguration)
            {
                if (dataPoint.Name == "Aqusition")
                {
                    item = new CollectItem(dataPoint.GidGeneratora, dataPoint.RawValue, dataPoint.Timestamp/*, dataPoint.Value.GidGeneratora*/);
                    key = new Tuple<long, DateTime>(item.Gid, item.Timestamp);
                    collectItems.Add(key, item);
                }
            }

            return collectItems;
        }

        #region Hour
        public double MinProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, CollectItem> dayItems, long key)//u kom satu u toku dana je minimalna vrednost ovog dera
        {
            double minPerHour = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key) && d.Value.P < minPerHour)
                    minPerHour = d.Value.P;
            }

            return minPerHour;
        }
        public double MaxProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, CollectItem> collectItems, long key)
        {
            double maxPerHour = double.MinValue;
            foreach (var d in collectItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key) && d.Value.P > maxPerHour)
                    maxPerHour = d.Value.P;
            }

            return maxPerHour;
        }
        public double AvgProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, CollectItem> collectItems, long key)
        {
            int counter = 0;
            double sumPerHour = 0;
            foreach (var d in collectItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerHour += d.Value.P;
                }
            }

            return sumPerHour / counter;
        }
        #endregion

        #region Day
        public double MinProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DayItem> dayItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerDay = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key) && d.Value.PMin < minPerDay)
                    minPerDay = d.Value.PMin;
            }

            return minPerDay;
        }
        public double MaxProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DayItem> dayItems, long key)
        {
            double maxPerDay = double.MinValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key) && d.Value.PMax > maxPerDay)
                    maxPerDay = d.Value.PMax;
            }

            return maxPerDay;
        }
        public double AvgProductionPerDay(int day, int month, Dictionary<Tuple<long, DateTime>, DayItem> dayItems, long key)
        {
            int counter = 0;
            double sumPerDay = 0;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Month.Equals(month) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerDay += d.Value.PAvg;
                }
            }

            return sumPerDay / counter;
        }
        #endregion

        #region Month
        public double MinProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, MonthItem> monthItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerMonth = double.MaxValue;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key) && d.Value.PMin < minPerMonth)
                    minPerMonth = d.Value.PMin;
            }

            return minPerMonth;
        }
        public double MaxProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, MonthItem> monthItems, long key)
        {
            double maxPerMonth = double.MinValue;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key) && d.Value.PMax > maxPerMonth)
                    maxPerMonth = d.Value.PMax;
            }

            return maxPerMonth;
        }
        public double AvgProductionPerMonth(int month, int year, Dictionary<Tuple<long, DateTime>, MonthItem> monthItems, long key)
        {
            int counter = 0;
            double sumPerMonth = 0;
            foreach (var d in monthItems)
            {
                if (d.Key.Item2.Month.Equals(month) && d.Key.Item2.Year.Equals(year) && d.Key.Item1.Equals(key))
                {
                    counter++;
                    sumPerMonth += d.Value.PAvg;
                }
            }

            return sumPerMonth / counter;
        }
        #endregion

    }
}
