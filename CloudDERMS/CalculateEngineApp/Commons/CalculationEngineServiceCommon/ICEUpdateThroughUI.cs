using DERMSCommon.DataModel.Core;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineServiceCommon
{
    [ServiceContract]
    public interface ICEUpdateThroughUI
    {
        [OperationContract]
        Task<float> UpdateThroughUI(long data);
        [OperationContract]
        Task<float> Balance(Dictionary<long, DerForecastDayAhead> prod, long GidUi, Dictionary<long, IdentifiedObject> networkModel, List<long> TurnedOffGenerators);
        [OperationContract]
        Task<float> BalanceNetworkModel();
        [OperationContract]
        Task<List<long>> AllGeoRegions();
        [OperationContract]
        Task<List<long>> AllowOptimization(long gid);
        [OperationContract]
        Task<List<long>> ListOfDisabledGenerators();
        [OperationContract]
        Task<List<Generator>> ListOffTurnedOffGenerators();
        [OperationContract]
        [ServiceKnownType(typeof(Generator))]
        Task<List<Generator>> GeneratorOffCheck();
    }
}
