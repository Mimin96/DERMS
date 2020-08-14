using DERMSCommon.DataModel.Core;
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
	public interface IDERFlexibility
	{
		[OperationContract]
		bool CheckFlexibility(long gid);
		[OperationContract]
		void TurnOnFlexibility(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid);
		[OperationContract]
		bool CheckFlexibilityForManualCommanding(long gid, Dictionary<long, IdentifiedObject> model);
		[OperationContract]
		void CalculateNewDerForecastDayAheadForNetworkModel(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TurnOnFlexibilityForNetworkModel(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		void CalculateNewDerForecastDayAheadForGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TurnOnFlexibilityForGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		void CalculateNewDerForecastDayAheadForSubGeoRegion(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TurnOnFlexibilityForSubGeoRegion(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		void CalculateNewDerForecastDayAheadForSubstation(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TurnOnFlexibilityForSubstation(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		void CalculateNewDerForecastDayAheadForGenerator(double flexibilityValue, Dictionary<long, DerForecastDayAhead> derForcast, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
		[OperationContract]
		Dictionary<long, double> TurnOnFlexibilityForGenerator(double flexibilityValue, long gid, Dictionary<long, IdentifiedObject> affectedEntities);
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
