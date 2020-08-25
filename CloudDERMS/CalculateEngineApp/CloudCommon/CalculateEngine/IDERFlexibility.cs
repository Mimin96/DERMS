using DERMSCommon.DataModel.Core;
using DERMSCommon.WeatherForecast;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.CalculateEngine
{
	[ServiceContract]
	public interface IDERFlexibility
	{
		[OperationContract]
		bool CheckFlexibility(long gid);
		[OperationContract]
		void TurnOnFlexibility(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid);
		[OperationContract]
		Task<bool> CheckFlexibilityForManualCommanding(long gid, Dictionary<long, IdentifiedObject> model);
		[OperationContract]
		Task CalculateNewDerForecastDayAheadForNetworkModel(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task<Dictionary<long, double>> TurnOnFlexibilityForNetworkModel(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task CalculateNewDerForecastDayAheadForGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task<Dictionary<long, double>> TurnOnFlexibilityForGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task CalculateNewDerForecastDayAheadForSubGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task<Dictionary<long, double>> TurnOnFlexibilityForSubGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task CalculateNewDerForecastDayAheadForSubstation(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task<Dictionary<long, double>> TurnOnFlexibilityForSubstation(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task CalculateNewDerForecastDayAheadForGenerator(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Task<Dictionary<long, double>> TurnOnFlexibilityForGenerator(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		List<Generator> GetGenerators();
		[OperationContract]
		List<Generator> GetGeneratorsForManualCommand(Dictionary<long, IdentifiedObject> nmsModel);
		[OperationContract]
		Dictionary<long, double> TempFlexibilityNetworkModel(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TempFlexibilityGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TempFlexibilitySubGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TempFlexibilitySubstation(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
	}
}
