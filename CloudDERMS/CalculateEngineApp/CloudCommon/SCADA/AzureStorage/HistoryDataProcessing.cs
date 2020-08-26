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
        public List<CollectItem> ConvertDataPoints(List<DataPoint> pointTypeToConfiguration)
        {
            List<CollectItem> collectItems = new List<CollectItem>();
            Tuple<long, DateTime> key;
            CollectItem item;

            foreach (var dataPoint in pointTypeToConfiguration)
            {
                if (dataPoint.Name == "Aqusition")
                {
                    item = new CollectItem(dataPoint.GidGeneratora, dataPoint.RawValue, dataPoint.Timestamp/*, dataPoint.Value.GidGeneratora*/);
                    key = new Tuple<long, DateTime>(item.Gid, item.Timestamp.DateTime);
                    collectItems.Add(item);
                }
            }

            return collectItems;
        }

        public List<DayItem> CollectTableToDayItems(List<CollectItem> collectItems)
        {
            List<DayItem> dayItems = new List<DayItem>();
            DayItem dayItem;

            foreach (var d in collectItems)
            {
                dayItem = new DayItem(d.Gid,
                                      d.Timestamp.Date.AddHours(d.Timestamp.Hour),
                                      MinProductionPerHour(d.Timestamp.Hour, d.Timestamp.DayOfYear, collectItems, d.Gid),
                                      MaxProductionPerHour(d.Timestamp.Hour, d.Timestamp.DayOfYear, collectItems, d.Gid),
                                      AvgProductionPerHour(d.Timestamp.Hour, d.Timestamp.DayOfYear, collectItems, d.Gid),
                                      0,
                                      d.P);

                if (dayItems.Where(x => x.Gid == dayItem.Gid && x.Timestamp == dayItem.Timestamp).FirstOrDefault() == null)
                    dayItems.Add(dayItem);
            }


            return dayItems;
        }

        public List<MonthItem> DayItemsToMonthItems(List<DayItem> dayItems)
        {
            List<MonthItem> monthItems = new List<MonthItem>();
            MonthItem monthItem;

            foreach (var d in dayItems)
            {
                monthItem = new MonthItem(d.Gid,
                                          d.Timestamp.Date,
                                          MinProductionPerDay(d.Timestamp.DayOfYear, d.Timestamp.Month, dayItems, d.Gid),
                                          MaxProductionPerDay(d.Timestamp.DayOfYear, d.Timestamp.Month, dayItems, d.Gid),
                                          AvgProductionPerDay(d.Timestamp.DayOfYear, d.Timestamp.Month, dayItems, d.Gid),
                                          0,
                                          d.P);

                if (monthItems.Where(x => x.Gid == monthItem.Gid && x.Timestamp == monthItem.Timestamp).FirstOrDefault() == null)
                    monthItems.Add(monthItem);
            }

            return monthItems;
        }

        public List<YearItem> MonthItemsToYearItems(List<MonthItem> monthItems)
        {
            List<YearItem> yearItems = new List<YearItem>();
            YearItem yearItem;
            bool ok;

            foreach (var d in monthItems)
            {
                ok = false;
                yearItem = new YearItem(d.Gid,
                                        d.Timestamp.Date,
                                        MinProductionPerMonth(d.Timestamp.Month, d.Timestamp.Year, monthItems, d.Gid),
                                        MaxProductionPerMonth(d.Timestamp.Month, d.Timestamp.Year, monthItems, d.Gid),
                                        AvgProductionPerMonth(d.Timestamp.Month, d.Timestamp.Year, monthItems, d.Gid),
                                        0,
                                        d.P);

                if (yearItems.Count > 0)
                {
                    foreach (var y in yearItems)
                    {
                        if (!(y.Timestamp.Month == d.Timestamp.Month && y.Gid == d.Gid))
                            ok = true;
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                        yearItems.Add(yearItem);
                }
                else
                    yearItems.Add(yearItem);
            }

            return yearItems;
        }

        #region Hour
        public double MinProductionPerHour(int hour, int day, List<CollectItem> collectItems, long key)//u kom satu u toku dana je minimalna vrednost ovog dera
        {
            double minPerHour = double.MaxValue;
            foreach (var d in collectItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Gid.Equals(key) && d.P < minPerHour)
                    minPerHour = d.P;
            }

            return minPerHour;
        }
        public double MaxProductionPerHour(int hour, int day, List<CollectItem> collectItems, long key)
        {
            double maxPerHour = double.MinValue;
            foreach (var d in collectItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Gid.Equals(key) && d.P > maxPerHour)
                    maxPerHour = d.P;
            }

            return maxPerHour;
        }
        public double AvgProductionPerHour(int hour, int day, List<CollectItem> collectItems, long key)
        {
            int counter = 0;
            double sumPerHour = 0;
            foreach (var d in collectItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Gid.Equals(key))
                {
                    counter++;
                    sumPerHour += d.P;
                }
            }

            return sumPerHour / counter;
        }
        #endregion

        #region Day
        public double MinProductionPerDay(int day, int month, List<DayItem> dayItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerDay = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Timestamp.Month.Equals(month) && d.Gid.Equals(key) && d.PMin < minPerDay)
                    minPerDay = d.PMin;
            }

            return minPerDay;
        }
        public double MaxProductionPerDay(int day, int month, List<DayItem> dayItems, long key)
        {
            double maxPerDay = double.MinValue;
            foreach (var d in dayItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Timestamp.Month.Equals(month) && d.Gid.Equals(key) && d.PMax > maxPerDay)
                    maxPerDay = d.PMax;
            }

            return maxPerDay;
        }
        public double AvgProductionPerDay(int day, int month, List<DayItem> dayItems, long key)
        {
            int counter = 0;
            double sumPerDay = 0;
            foreach (var d in dayItems)
            {
                if (d.Timestamp.DayOfYear.Equals(day) && d.Timestamp.Month.Equals(month) && d.Gid.Equals(key))
                {
                    counter++;
                    sumPerDay += d.PAvg;
                }
            }

            return sumPerDay / counter;
        }
        #endregion

        #region Month
        public double MinProductionPerMonth(int month, int year, List<MonthItem> monthItems, long key)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerMonth = double.MaxValue;
            foreach (var d in monthItems)
            {
                if (d.Timestamp.Month.Equals(month) && d.Timestamp.Year.Equals(year) && d.Gid.Equals(key) && d.PMin < minPerMonth)
                    minPerMonth = d.PMin;
            }

            return minPerMonth;
        }
        public double MaxProductionPerMonth(int month, int year, List<MonthItem> monthItems, long key)
        {
            double maxPerMonth = double.MinValue;
            foreach (var d in monthItems)
            {
                if (d.Timestamp.Month.Equals(month) && d.Timestamp.Year.Equals(year) && d.Gid.Equals(key) && d.PMax > maxPerMonth)
                    maxPerMonth = d.PMax;
            }

            return maxPerMonth;
        }
        public double AvgProductionPerMonth(int month, int year, List<MonthItem> monthItems, long key)
        {
            int counter = 0;
            double sumPerMonth = 0;
            foreach (var d in monthItems)
            {
                if (d.Timestamp.Month.Equals(month) && d.Timestamp.Year.Equals(year) && d.Gid.Equals(key))
                {
                    counter++;
                    sumPerMonth += d.PAvg;
                }
            }

            return sumPerMonth / counter;
        }
        #endregion

    }
}
