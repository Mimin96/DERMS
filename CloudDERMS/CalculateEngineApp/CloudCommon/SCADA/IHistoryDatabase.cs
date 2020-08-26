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
        List<CollectItem> GetCollectItems();
        [OperationContract]
        List<DayItem> GetDayItems();
        [OperationContract]
        List<MonthItem> GetMonthItems();
        [OperationContract]
        List<YearItem> GetYearItems();
    }
}
