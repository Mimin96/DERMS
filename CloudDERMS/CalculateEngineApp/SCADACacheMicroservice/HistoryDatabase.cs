using CloudCommon.SCADA;
using CloudCommon.SCADA.AzureStorage;
using CloudCommon.SCADA.AzureStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACacheMicroservice
{
    public class HistoryDatabase : IHistoryDatabase
    {
        public List<CollectItemUI> GetCollectItems()
        {
            List<CollectItem> collectItems = AzureTableStorage.GetAllCollectItems("UseDevelopmentStorage=true;", "CollectItems");
            List<CollectItemUI> collectItemUIs = new List<CollectItemUI>();

            foreach (CollectItem collectItem in collectItems)
            {
                collectItemUIs.Add(new CollectItemUI(collectItem.Gid, collectItem.P, collectItem.DateTime));
            }

            return collectItemUIs;
        }

        public List<DayItemUI> GetDayItems()
        {
            List<DayItem> dayItems = AzureTableStorage.GetAllDayItems("UseDevelopmentStorage=true;", "DayItems");
            List<DayItemUI> dayItemUIs = new List<DayItemUI>();

            foreach (DayItem dayItem in dayItems)
            {
                dayItemUIs.Add(new DayItemUI(dayItem.Gid, dayItem.DateTime, dayItem.PMin, dayItem.PMax, dayItem.PAvg, dayItem.E, dayItem.P));
            }

            return dayItemUIs;
        }

        public List<MonthItemUI> GetMonthItems()
        {
            List<MonthItem> monthItems = AzureTableStorage.GetAllMonthItems("UseDevelopmentStorage=true;", "MonthItems");
            List<MonthItemUI> monthItemUIs = new List<MonthItemUI>();

            foreach (MonthItem monthItem in monthItems)
            {
                monthItemUIs.Add(new MonthItemUI(monthItem.Gid, monthItem.DateTime, monthItem.PMin, monthItem.PMax, monthItem.PAvg, monthItem.E, monthItem.P));
            }

            return monthItemUIs;
        }

        public List<YearItemUI> GetYearItems()
        {
            List<YearItem> yearItems = AzureTableStorage.GetAllYearItems("UseDevelopmentStorage=true;", "YearItems");
            List<YearItemUI> yearItemUIs = new List<YearItemUI>();

            foreach (YearItem yearItem in yearItems)
            {
                yearItemUIs.Add(new YearItemUI(yearItem.Gid, yearItem.DateTime, yearItem.PMin, yearItem.PMax, yearItem.PAvg, yearItem.E, yearItem.P));
            }

            return yearItemUIs;
        }

        public List<CollectItemUI> GetCollectItemsDateTime(DateTime dateTime, long gid)
        {
            List<CollectItem> collectItems = AzureTableStorage.GetCollectItems("UseDevelopmentStorage=true;", "CollectItems",
                                                                                                                            dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day
                                                                                                                            );
            List<CollectItemUI> collectItemUIs = new List<CollectItemUI>();

            foreach (CollectItem collectItem in collectItems)
            {
                if (collectItem.Gid == gid)
                    collectItemUIs.Add(new CollectItemUI(collectItem.Gid, collectItem.P, collectItem.DateTime));
            }

            return collectItemUIs;
        }

        public List<DayItemUI> GetDayItemsDateTime(DateTime dateTime, long gid)
        {
            List<DayItem> dayItems = AzureTableStorage.GetDayItems("UseDevelopmentStorage=true;", "DayItems",
                                                                                                              dateTime.Year + "-" + dateTime.Month
                                                                                                              );
            List<DayItemUI> dayItemUIs = new List<DayItemUI>();

            foreach (DayItem dayItem in dayItems)
            {
                if (dayItem.Gid == gid)
                    dayItemUIs.Add(new DayItemUI(dayItem.Gid, dayItem.DateTime, dayItem.PMin, dayItem.PMax, dayItem.PAvg, dayItem.E, dayItem.P));
            }

            return dayItemUIs;
        }

        public List<MonthItemUI> GetMonthItemsDateTime(DateTime dateTime, long gid)
        {
            List<MonthItem> monthItems = AzureTableStorage.GetMonthItems("UseDevelopmentStorage=true;", "MonthItems",
                                                                                                                    dateTime.Year.ToString()
                                                                                                                    );
            List<MonthItemUI> monthItemUIs = new List<MonthItemUI>();

            foreach (MonthItem monthItem in monthItems)
            {
                if (monthItem.Gid == gid)
                    monthItemUIs.Add(new MonthItemUI(monthItem.Gid, monthItem.DateTime, monthItem.PMin, monthItem.PMax, monthItem.PAvg, monthItem.E, monthItem.P));
            }

            return monthItemUIs;
        }
    }
}
