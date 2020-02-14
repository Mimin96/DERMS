using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using FTN.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecast;

namespace CalculationEngineService
{
    public class CalculationEngineCache
    {
        private Dictionary<long, IdentifiedObject> nmsCache = new Dictionary<long, IdentifiedObject>();
        private Dictionary<long, List<DataPoint>> scadaPointsCached = new Dictionary<long, List<DataPoint>>();
        private Dictionary<long, Forecast> derWeatherCached = new Dictionary<long, Forecast>();
        private Dictionary<long, DerForecastDayAhead> productionCached = new Dictionary<long, DerForecastDayAhead>();

        private static CalculationEngineCache instance = null;

        public static CalculationEngineCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CalculationEngineCache();
                }

                return instance;

            }
        }

        public void PopulateNSMModelCache(NetworkModelTransfer networkModelTransfer)
        {
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Delete)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Insert)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Update)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }
        }

        public void PopulateWeatherForecast(NetworkModelTransfer networkModel)
        {
            //KRUZNA REFERENCA PROBLEM!!!!!!!
            WeatherForecast.DarkSkyApi darkSkyApi = new WeatherForecast.DarkSkyApi();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                }
            }
        }
        public void PopulateProductionForecast(NetworkModelTransfer networkModel)
        {
            ProductionCalculator productionCalculator = new ProductionCalculator(networkModel);
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        AddDerForecast(productionCalculator.CalculateSubstation(GetForecast(kvpDic.Key), gr), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
            PubSubCalculatioEngine.Instance.Notify(productionCached, (int)Enums.Topics.Default); // KAD SE POPUNI CACHE SALJE SVIMA Dictionary
        }

        public void PopulateConsumptionForecast(NetworkModelTransfer networkModel)
        {
            ConsumptionCalculator consumptionCalculator = new ConsumptionCalculator(networkModel);
            consumptionCalculator.Calculate(productionCached);
            PubSubCalculatioEngine.Instance.Notify(productionCached, (int)Enums.Topics.Default);
        }

        public List<DataPoint> GetDataPoints(long gid)
        {
            if (!scadaPointsCached.ContainsKey(gid))
                return null;
            return scadaPointsCached[gid];
        }

        public Forecast GetForecast(long gid)
        {
            if (!derWeatherCached.ContainsKey(gid))
                return null;
            return derWeatherCached[gid];
        }

        public DerForecastDayAhead GetDerForecastDayAhead(long gid)
        {
            if (!productionCached.ContainsKey(gid))
                return null;
            return productionCached[gid];
        }

        public void AddForecast(Forecast wf, long gid)
        {
            if (!derWeatherCached.ContainsKey(gid))
                derWeatherCached.Add(gid, wf);
        }

        public void AddDerForecast(DerForecastDayAhead derForecastDayAhead, long gid, bool isInitState)
        {
            if (!productionCached.ContainsKey(gid))
                productionCached.Add(gid, derForecastDayAhead);

            if (!isInitState)
                PubSubCalculatioEngine.Instance.Notify(productionCached, (int)Enums.Topics.Default);
        }

        public void AddScadaPoints(List<DataPoint> dataPoints)
        {
            List<DataPoint> temp = new List<DataPoint>();
            foreach (DataPoint dp in dataPoints)
            {
                if (!scadaPointsCached.ContainsKey(dp.Gid))
                {
                    foreach (DataPoint dp1 in dataPoints)
                    {
                        if (dp.Gid.Equals(dp1.Gid))
                            temp.Add(dp1);
                    }
                }
                scadaPointsCached.Add(dp.Gid, new List<DataPoint>(temp));
                temp.Clear();
            }
        }

        public Dictionary<long, DerForecastDayAhead> GetAllDerForecastDayAhead()
        {
            return productionCached;
        }

        public void AddNMSModelEntity(IdentifiedObject io)
        {
            if (!nmsCache.ContainsKey(io.GlobalId))
                nmsCache.Add(io.GlobalId, io);
        }

        public void DeleteNMSModelEntity(IdentifiedObject io)
        {
            if (nmsCache.ContainsKey(io.GlobalId))
                nmsCache.Remove(io.GlobalId);
        }

        public void UpdateNMSModelEntity(IdentifiedObject io)
        {
            if (nmsCache.ContainsKey(io.GlobalId))
                nmsCache[io.GlobalId] = io;
        }

        public void RestartCache(NetworkModelTransfer networkModelTransfer)
        {
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Delete)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    DeleteNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Insert)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    AddNMSModelEntity(io);
                }
            }

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> dictionary in networkModelTransfer.Update)
            {
                foreach (IdentifiedObject io in dictionary.Value.Values)
                {
                    UpdateNMSModelEntity(io);
                }
            }
        }
    }
}
