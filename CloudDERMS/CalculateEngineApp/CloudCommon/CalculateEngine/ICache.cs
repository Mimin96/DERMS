using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
    [ServiceContract]
    public interface ICache
    {
        //MUST HAVE GET CREATE UPDATE DELETE 
        [OperationContract]
        void PopulateNSMModelCache(NetworkModelTransfer networkModelTransfer);
        [OperationContract]
        Task<Dictionary<long, IdentifiedObject>> GetNMSModel();
        [OperationContract]
        void RestartCache(NetworkModelTransfer networkModelTransfer);
        [OperationContract]
        void AddScadaPoints(List<DataPoint> dataPoints);
        [OperationContract]
        void PopulateWeatherForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        void PopulateConsumptionForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        void PopulateProductionForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        void CalculateNewFlexibility(DataToUI data);
        [OperationContract]
        DataToUI CreateDataForUI();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetAllDerForecastDayAhead();
        [OperationContract]
        void UpdateGraphWithScadaValues(List<DataPoint> data);
        [OperationContract]
        void UpdateNewDataPoitns(List<DataPoint> points);
        [OperationContract]
        void ApplyChangesOnProductionCached();
        [OperationContract]
        void SendDerForecastDayAhead();
        [OperationContract]
        Task<float> PopulateBalance(long gid);
        [OperationContract]
        void UpdateMinAndMaxFlexibilityForChangedGenerators();
        [OperationContract]
        Dictionary<long, List<DataPoint>> GetscadaPointsCached();
        [OperationContract]
        TreeNode<NodeData> GetGraph();
        [OperationContract]
        List<DataPoint> GetDatapoints();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetCopyOfProductionCached();
        [OperationContract]
        Dictionary<long, double> GetListOfGeneratorsForScada();
        [OperationContract]
        Task<Dictionary<int, List<long>>> GetDisableAutomaticOptimization();
        [OperationContract]
        Task<Dictionary<int, List<long>>> GetTurnedOffGenerators();
        [OperationContract]
        Task<Dictionary<int, List<long>>> GetTurnedOnGenerators();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetTempProductionCached();
        [OperationContract]
        Dictionary<long, DayAhead> GetSubstationDayAhead();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetSubstationsForecast();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetSubGeographicalRegionsForecast();
        [OperationContract]
        Dictionary<long, DerForecastDayAhead> GetGeneratorForecastList();
        [OperationContract]
        List<DataPoint> GetScadaDataPoint(long param);
        [OperationContract]
        DerForecastDayAhead GetDerForecast(long gid);
        [OperationContract]
        void AddDerForecastDayAhead(long id, DerForecastDayAhead forecast);
        [OperationContract]
        void AddToListOfGeneratorsForScada(long gid, double param);
        [OperationContract]
        void AddToTempProductionCached(long gid, DerForecastDayAhead param);
        [OperationContract]
        void AddToSubstationDayAhead(long gid, DayAhead param);
        [OperationContract]
        void AddToSubstationsForecast(long gid, DerForecastDayAhead param);
        [OperationContract]
        void AddToSubGeographicalRegionsForecast(long gid, DerForecastDayAhead param);
        [OperationContract]
        void AddToGeneratorForecastList(long gid, DerForecastDayAhead param);
        [OperationContract]
        void RemoveFromGeneratorForecastList(long gid);
        [OperationContract]
        void RemoveFromSubGeographicalRegionsForecast(long gid);
        [OperationContract]
        void RemoveFromSubstationsForecast(long gid);
        [OperationContract]
        void RemoveFromSubstationDayAhead(long gid);
        [OperationContract]
        void RemoveFromTempProductionCached(long gid);
        [OperationContract]
        void AddToCopyOfProductionCached(long gid, DerForecastDayAhead forecast);
        [OperationContract]
        void AddToTurnedOnGenerators(long param);
        [OperationContract]
        void RemoveFromTurnedOnGenerators(long param);
        [OperationContract]
        void AddToTurnedOffGenerators(long param);
        [OperationContract]
        void RemoveFromTurnedOffGenerators(long param);
        [OperationContract]
        void AddToDisableAutomaticOptimization(long param);
        [OperationContract]
        void RemoveFromDisableAutomaticOptimization(long param);
        [OperationContract]
        void RemoveFromListOfGeneratorsForScada(long gid);
        [OperationContract]
        void RemoveFromCopyOfProductionCached(long gid);
        [OperationContract]
        void AddToDataPoints(DataPoint datapoint);
        [OperationContract]
        void RemoveFromDataPoints(DataPoint datapoint);
        [OperationContract]
        void RemoveFromDerForecastDayAhead(long id);
        [OperationContract]
        void RemoveFromDerForecast(long gid);
        [OperationContract]
        void RemoveFromScadaDataPoint(long gid);

    }
}
