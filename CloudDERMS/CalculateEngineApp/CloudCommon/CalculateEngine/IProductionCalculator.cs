﻿using DarkSkyApi.Models;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
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
    public interface IProductionCalculator
    {
        [OperationContract]
        Task<DerForecastDayAhead> CalculateGenerator(Forecast forecast, Generator generator, Dictionary<long, DerForecastDayAhead> GeneratorForecastList);
        [OperationContract]
        Task<DerForecastDayAhead> CalculateSubstation(Forecast forecast, Substation substation, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> GeneratorForecastList, Dictionary<long, DerForecastDayAhead> SubstationsForecast);
        [OperationContract]
        Task<DerForecastDayAhead> CalculateSubRegion(SubGeographicalRegion subGeographicalRegion, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> SubstationsForecast, Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast);
        [OperationContract]
        Task<DerForecastDayAhead> CalculateGeoRegion(GeographicalRegion geographicalRegion, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast);


    }
}