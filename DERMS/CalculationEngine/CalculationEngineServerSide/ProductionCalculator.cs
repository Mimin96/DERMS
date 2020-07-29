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

namespace CalculationEngineService
{
    public class ProductionCalculator
    {

        public DerForecastDayAhead CalculateGenerator(Forecast forecast, Generator generator)
        {
            DerForecastDayAhead generatorForecast = new DerForecastDayAhead(generator.GlobalId);

            DayAhead dayAhead = generator.CalculateDayAhead(forecast, generator.GlobalId, new Substation(generator.GlobalId));
            generatorForecast.Production += dayAhead;
            CalculationEngineCache.Instance.GeneratorForecastList[generator.GlobalId] = generatorForecast;
            return generatorForecast;
        }

        public DerForecastDayAhead CalculateSubstation(Forecast forecast, Substation substation, NetworkModelTransfer networkModel)
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
                    substationForecast.Production += CalculationEngineCache.Instance.GeneratorForecastList[generator.GlobalId].Production;

                    CalculationEngineCache.Instance.SubstationsForecast[substation.GlobalId] = substationForecast;
                }
            }
            return substationForecast;
        }

        public DerForecastDayAhead CalculateSubRegion(SubGeographicalRegion subGeographicalRegion, NetworkModelTransfer networkModel)
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

                    subGeographicalRegionForecast.Production += CalculationEngineCache.Instance.SubstationsForecast[substation.GlobalId].Production;

                    CalculationEngineCache.Instance.SubGeographicalRegionsForecast[subGeographicalRegion.GlobalId] = subGeographicalRegionForecast;
                }
            }
            return subGeographicalRegionForecast;
        }

        public DerForecastDayAhead CalculateGeoRegion(GeographicalRegion geographicalRegion, NetworkModelTransfer networkModel)
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
                    geoRegionForecast.Production += CalculationEngineCache.Instance.SubGeographicalRegionsForecast[subGeo.GlobalId].Production;
                }
            }
            return geoRegionForecast;
        }
    }

}
