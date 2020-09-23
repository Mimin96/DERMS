using CloudCommon.SCADA.AzureStorage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.SCADA
{
    [ServiceContract]
    public interface IHistoryDatabase
    {
        [OperationContract]
        List<CollectItemUI> GetCollectItems();
        [OperationContract]
        List<DayItemUI> GetDayItems();
        [OperationContract]
        List<MonthItemUI> GetMonthItems();
        [OperationContract]
        List<YearItemUI> GetYearItems();
    }
}
