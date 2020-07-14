using DERMSCommon.DataModel.Core;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    public class IslandCalculations
    {
        private Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
        public void GeneratorOff(long generatorGid, Dictionary<long, DerForecastDayAhead> prod)
        {
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
            foreach (long gid in networkModel.Keys)
            {
                IdentifiedObject io = networkModel[gid];
                var type = io.GetType();

                if (type.Name.Equals("Substation"))
                {
                    Substation substation = (Substation)networkModel[gid];
                    if (substation.Equipments.Contains(generatorGid))
                    {
                        prod[gid].Production -= prod[generatorGid].Production;
                        SubGeographicalRegion subgr = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                        prod[subgr.GlobalId].Production -= prod[generatorGid].Production;
                        prod[subgr.GeoReg].Production -= prod[generatorGid].Production;

                    }
                }
            }
        }
        public void GeneratorOn(long generatorGid, Dictionary<long, DerForecastDayAhead> prod)
        {
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
            foreach (long gid in networkModel.Keys)
            {
                IdentifiedObject io = networkModel[gid];
                var type = io.GetType();

                if (type.Name.Equals("Substation"))
                {
                    Substation substation = (Substation)networkModel[gid];
                    if (substation.Equipments.Contains(generatorGid))
                    {
                        prod[gid].Production += prod[generatorGid].Production;
                        SubGeographicalRegion subgr = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                        prod[subgr.GlobalId].Production += prod[generatorGid].Production;
                        prod[subgr.GeoReg].Production += prod[generatorGid].Production;

                    }
                }
            }
        }
    }
}
