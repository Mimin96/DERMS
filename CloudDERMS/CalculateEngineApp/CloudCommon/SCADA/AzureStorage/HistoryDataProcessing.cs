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

        public DayItem CollectTableToDayItems(List<CollectItem> collectItems)
        {
            DayItem dayItem;

            dayItem = new DayItem(collectItems[0].Gid,
                                    collectItems[0].Timestamp.DateTime,
                                    MinProductionPerHour(collectItems),
                                    MaxProductionPerHour(collectItems),
                                    AvgProductionPerHour(collectItems),
                                    0,
                                    0);


            return dayItem;
        }

        public MonthItem DayItemsToMonthItems(List<DayItem> dayItems)
        {
            MonthItem monthItem;

            monthItem = new MonthItem(dayItems[0].Gid,
                                        dayItems[0].Timestamp.DateTime,
                                        MinProductionPerDay(dayItems),
                                        MaxProductionPerDay(dayItems),
                                        AvgProductionPerDay(dayItems),
                                        0,
                                        0);

            return monthItem;
        }

        public YearItem MonthItemsToYearItems(List<MonthItem> monthItems)
        {
            YearItem yearItem;

            yearItem = new YearItem(monthItems[0].Gid,
                                    monthItems[0].Timestamp.DateTime,
                                    MinProductionPerMonth(monthItems),
                                    MaxProductionPerMonth(monthItems),
                                    AvgProductionPerMonth(monthItems),
                                    0,
                                    0);

            return yearItem;
        }

        #region Hour
        public double MinProductionPerHour(List<CollectItem> collectItems)//u kom satu u toku dana je minimalna vrednost ovog dera
        {
            double minPerHour = double.MaxValue;
            foreach (var d in collectItems)
            {
                if (d.P < minPerHour)
                    minPerHour = d.P;
            }

            return minPerHour;
        }
        public double MaxProductionPerHour(List<CollectItem> collectItems)
        {
            double maxPerHour = double.MinValue;
            foreach (var d in collectItems)
            {
                if (d.P > maxPerHour)
                    maxPerHour = d.P;
            }

            return maxPerHour;
        }
        public double AvgProductionPerHour(List<CollectItem> collectItems)
        {
            double sumPerHour = 0;
            foreach (var d in collectItems)
            {
                    sumPerHour += d.P;
            }

            return sumPerHour / collectItems.Count;
        }
        #endregion

        #region Day
        public double MinProductionPerDay(List<DayItem> dayItems)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerDay = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.PMin < minPerDay)
                    minPerDay = d.PMin;
            }

            return minPerDay;
        }
        public double MaxProductionPerDay(List<DayItem> dayItems)
        {
            double maxPerDay = double.MinValue;
            foreach (var d in dayItems)
            {
                if (d.PMax > maxPerDay)
                    maxPerDay = d.PMax;
            }

            return maxPerDay;
        }
        public double AvgProductionPerDay(List<DayItem> dayItems)
        {
            double sumPerDay = 0;
            foreach (var d in dayItems)
            {
                    sumPerDay += d.PAvg;
            }

            return sumPerDay / dayItems.Count;
        }
        #endregion

        #region Month
        public double MinProductionPerMonth(List<MonthItem> monthItems)//u kom danu u toku meseca je minimalna vrednost ovog dera
        {
            double minPerMonth = double.MaxValue;
            foreach (var d in monthItems)
            {
                if (d.PMin < minPerMonth)
                    minPerMonth = d.PMin;
            }

            return minPerMonth;
        }
        public double MaxProductionPerMonth(List<MonthItem> monthItems)
        {
            double maxPerMonth = double.MinValue;
            foreach (var d in monthItems)
            {
                if (d.PMax > maxPerMonth)
                    maxPerMonth = d.PMax;
            }

            return maxPerMonth;
        }
        public double AvgProductionPerMonth(List<MonthItem> monthItems)
        {
            double sumPerMonth = 0;
            foreach (var d in monthItems)
            {
                    sumPerMonth += d.PAvg;
            }

            return sumPerMonth / monthItems.Count;
        }
        #endregion

    }
}
