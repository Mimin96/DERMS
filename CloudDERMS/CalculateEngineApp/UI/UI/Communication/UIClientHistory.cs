using CloudCommon.SCADA;
using CloudCommon.SCADA.AzureStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UI.Communication
{
    public class UIClientHistory : ClientBase<IHistoryDatabase>, IHistoryDatabase
    {
        public UIClientHistory()
        {

        }

        public UIClientHistory(string endpoint) : base(endpoint)
        {

        }

        public List<CollectItemUI> GetCollectItems()
        {
            return Channel.GetCollectItems();
        }

        public List<CollectItemUI> GetCollectItemsDateTime(DateTime dateTime, long gid)
        {
            return Channel.GetCollectItemsDateTime(dateTime, gid);
        }

        public List<DayItemUI> GetDayItems()
        {
            return Channel.GetDayItems();
        }

        public List<DayItemUI> GetDayItemsDateTime(DateTime dateTime, long gid)
        {
            return Channel.GetDayItemsDateTime(dateTime, gid);
        }

        public List<MonthItemUI> GetMonthItems()
        {
            return Channel.GetMonthItems();
        }

        public List<MonthItemUI> GetMonthItemsDateTime(DateTime dateTime, long gid)
        {
            return Channel.GetMonthItemsDateTime(dateTime, gid);
        }

        public List<YearItemUI> GetYearItems()
        {
            return Channel.GetYearItems();
        }
    }
}
