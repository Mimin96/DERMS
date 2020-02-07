using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
    public class CalculationEngineCache
    {
        private Dictionary<long, IdentifiedObject> nmsCache = new Dictionary<long, IdentifiedObject>();
        private Dictionary<long, List<DataPoint>> scadaPointsCached = new Dictionary<long, List<DataPoint>>();
        private Dictionary<long, WeatherForecast.WeatherForecast> derWeatherCached = new Dictionary<long, WeatherForecast.WeatherForecast>();

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
            DarkSkyApi darkSkyApi = new DarkSkyApi(); 
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("ConductingEquipment"))
                    {
                        var gr = (ConductingEquipment)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("EquipmentContainer"))
                    {
                        var gr = (EquipmentContainer)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(gr.Latitude, gr.Longitude).Result, kvpDic.Key);
                    }
                }
            }
        }

        public List<DataPoint> GetDataPoints(long gid)
        {
            if (!scadaPointsCached.ContainsKey(gid))
                return null;
            return scadaPointsCached[gid];
        }

        public WeatherForecast.WeatherForecast GetForecast(long gid)
        {
            if (!derWeatherCached.ContainsKey(gid))
                return null;
            return derWeatherCached[gid];
        }

        public void AddForecast(WeatherForecast.WeatherForecast wf, long gid)
        {
            if (!derWeatherCached.ContainsKey(gid))
                derWeatherCached.Add(gid, wf);
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
