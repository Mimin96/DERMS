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
        public List<CollectItem> GetCollectItems()
        {
            return AzureTableStorage.GetAllCollectItems("UseDevelopmentStorage=true;", "CollectItems");
        }

        public List<DayItem> GetDayItems()
        {
            return AzureTableStorage.GetAllDayItems("UseDevelopmentStorage=true;", "DayItems");
        }

        public List<MonthItem> GetMonthItems()
        {
            return AzureTableStorage.GetAllMonthItems("UseDevelopmentStorage=true;", "MonthItems");
        }

        public List<YearItem> GetYearItems()
        {
            return AzureTableStorage.GetAllYearItems("UseDevelopmentStorage=true;", "YearItems");
        }
    }
}
