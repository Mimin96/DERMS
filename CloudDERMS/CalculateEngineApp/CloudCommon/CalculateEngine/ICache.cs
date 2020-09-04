using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
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
        Task PopulateNSMModelCache(NetworkModelTransfer networkModelTransfer);
        [OperationContract]
        Task<Dictionary<long, IdentifiedObject>> GetNMSModel();
        [OperationContract]
        void RestartCache(NetworkModelTransfer networkModelTransfer);
        [OperationContract]
        void AddScadaPoints(List<DataPoint> dataPoints);
        [OperationContract]
        Task PopulateWeatherForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        Task PopulateConsumptionForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        Task PopulateProductionForecast(NetworkModelTransfer networkModel);
        [OperationContract]
        DataToUI CreateDataForUI();
        [OperationContract]
        Task<Dictionary<long, DerForecastDayAhead>> GetAllDerForecastDayAhead();
        [OperationContract]
        void UpdateGraphWithScadaValues(List<DataPoint> data);
        [OperationContract]
        void UpdateNewDataPoitns(List<DataPoint> points);
        [OperationContract]
        Task ApplyChangesOnProductionCached(Dictionary<long, double> listOfGeneratorsForScada);
        [OperationContract]
        Task SendDerForecastDayAhead();
        [OperationContract]
        Task<float> PopulateBalance(long gid);
        [OperationContract]
        Task UpdateMinAndMaxFlexibilityForChangedGenerators(Dictionary<long, double> listOfGeneratorsForScada);
        [OperationContract]
        Dictionary<long, List<DataPoint>> GetscadaPointsCached();
        [OperationContract]
        Task<TreeNode<NodeData>> GetGraph();
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
        Task<Dictionary<long, DerForecastDayAhead>> GetTempProductionCached();
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
        Task AddToListOfGeneratorsForScada(long gid, double param);
        [OperationContract]
        Task AddToTempProductionCached(long gid, DerForecastDayAhead param);
        [OperationContract]
        Task AddToSubstationDayAhead(long gid, DayAhead param);
        [OperationContract]
        Task AddToSubstationsForecast(long gid, DerForecastDayAhead param);
        [OperationContract]
        Task AddToSubGeographicalRegionsForecast(long gid, DerForecastDayAhead param);
        [OperationContract]
        Task AddToGeneratorForecastList(long gid, DerForecastDayAhead param);
        [OperationContract]
        void RemoveFromGeneratorForecastList(long gid);
        [OperationContract]
        void RemoveFromSubGeographicalRegionsForecast(long gid);
        [OperationContract]
        void RemoveFromSubstationsForecast(long gid);
        [OperationContract]
        void RemoveFromSubstationDayAhead(long gid);
        [OperationContract]
        Task RemoveFromTempProductionCached(long gid);
        [OperationContract]
        void AddToCopyOfProductionCached(long gid, DerForecastDayAhead forecast);
        [OperationContract]
        Task AddToTurnedOnGenerators(long param);
        [OperationContract]
        Task RemoveFromTurnedOnGenerators(long param);
        [OperationContract]
        Task AddToTurnedOffGenerators(long param);
        [OperationContract]
        Task RemoveFromTurnedOffGenerators(long param);
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
        [OperationContract]
        Task SetNetworkModelTreeClass(List<NetworkModelTreeClass> networkModelTreeClass);
        [OperationContract]
        Task<List<NetworkModelTreeClass>> GetNetworkModelTreeClass();
        [OperationContract]
        Task<Dictionary<long, Forecast>> GetWholeDerWeatherCached();
        [OperationContract]
        Task<List<long>> GetDisableAutomaticOptimizationList();
        [OperationContract]
        Task CalculateNewCopyOfProductionCachedFlexibility(Dictionary<long, DerForecastDayAhead> copyOfProductionCachedFlexibility);
        [OperationContract]
        Task AddDerForecast(DerForecastDayAhead derForecastDayAhead, long gid, bool isInitState);
    }
}
