using CloudCommon.CalculateEngine;
using DarkSkyApi.Models;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECalculationMicroservice
{
    public class ProductionCalculatorService : IProductionCalculator
    {
        public async Task<DerForecastDayAhead> CalculateGenerator(Forecast forecast, Generator generator, Dictionary<long, DerForecastDayAhead> GeneratorForecastList)
        {
            DerForecastDayAhead generatorForecast = new DerForecastDayAhead(generator.GlobalId);

            DayAhead dayAhead = generator.CalculateDayAhead(forecast, generator.GlobalId, new Substation(generator.GlobalId));
            generatorForecast.Production += dayAhead;
            GeneratorForecastList[generator.GlobalId] = generatorForecast;///
            return generatorForecast;
        }

        public async Task<DerForecastDayAhead> CalculateSubstation(Forecast forecast, Substation substation, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> GeneratorForecastList, Dictionary<long, DerForecastDayAhead> SubstationsForecast)
        {

            List<Generator> generators = new List<Generator>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Generator"))
                    {
                        var generator = (Generator)kvpDic.Value;
                        generators.Add(generator);
                    }
                }
            }


            DerForecastDayAhead substationForecast = new DerForecastDayAhead(substation.GlobalId);
            foreach (Generator generator in generators)
            {
                if (substation.Equipments.Contains(generator.GlobalId))
                {
                    // DayAhead dayAhead = generator.CalculateDayAhead(forecast, substation.GlobalId, substation);
                    substationForecast.Production += GeneratorForecastList[generator.GlobalId].Production;

                    SubstationsForecast[substation.GlobalId] = substationForecast;
                }
            }
            return substationForecast;
        }

        public async Task<DerForecastDayAhead> CalculateSubRegion(SubGeographicalRegion subGeographicalRegion, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> SubstationsForecast, Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast)
        {
            List<Substation> substations = new List<Substation>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var substation = (Substation)kvpDic.Value;
                        substations.Add(substation);
                    }
                }
            }
            DerForecastDayAhead subGeographicalRegionForecast = new DerForecastDayAhead(subGeographicalRegion.GlobalId);
            foreach (Substation substation in substations)
            {
                if (subGeographicalRegion.Substations.Contains(substation.GlobalId))
                {

                    subGeographicalRegionForecast.Production += SubstationsForecast[substation.GlobalId].Production;

                    SubGeographicalRegionsForecast[subGeographicalRegion.GlobalId] = subGeographicalRegionForecast;
                }
            }
            return subGeographicalRegionForecast;
        }

        public async Task<DerForecastDayAhead> CalculateGeoRegion(GeographicalRegion geographicalRegion, NetworkModelTransfer networkModel, Dictionary<long, DerForecastDayAhead> SubGeographicalRegionsForecast)
        {
            List<SubGeographicalRegion> subGeographicalRegions = new List<SubGeographicalRegion>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var substation = (SubGeographicalRegion)kvpDic.Value;
                        subGeographicalRegions.Add(substation);
                    }
                }
            }
            DerForecastDayAhead geoRegionForecast = new DerForecastDayAhead(geographicalRegion.GlobalId);
            foreach (SubGeographicalRegion subGeo in subGeographicalRegions)
            {
                if (geographicalRegion.Regions.Contains(subGeo.GlobalId))
                {
                    geoRegionForecast.Production += SubGeographicalRegionsForecast[subGeo.GlobalId].Production;
                }
            }
            return geoRegionForecast;
        }
    }
}
