using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
using FTN.Common;

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private TreeNode<NodeData> graphCached;

        private Dictionary<long, DerForecastDayAhead> copyOfProductionCached = new Dictionary<long, DerForecastDayAhead>();
        private Dictionary<long, double> listOfGeneratorsForScada = new Dictionary<long, double>();

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
        public Dictionary<long,double> ListOfGenerators
        {
            get { return listOfGeneratorsForScada; }
            set { listOfGeneratorsForScada = value; }
        }
        public TreeNode<NodeData> GraphCached
        {
            get { return graphCached; }
            set { graphCached = value; }
        }
        public List<NetworkModelTreeClass> NetworkModelTreeClass
        {
            get;
            set;
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

            //temp poziv
            PopulateGraphCached(networkModelTransfer);
        }
        public void PopulateWeatherForecast(NetworkModelTransfer networkModel)
        {
            double lat, lon;
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
                    else if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
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
                    if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        AddDerForecast(productionCalculator.CalculateGenerator(GetForecast(kvpDic.Key), gr), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
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
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        AddDerForecast(productionCalculator.CalculateSubRegion(gr), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        AddDerForecast(productionCalculator.CalculateGeoRegion(gr), kvpDic.Key, true); // true DA NE BI ZA SVAKI DODATI DerForecastDayAhead PUB SUB SLAO SVIMA CEO Dictionary 
                    }
                }
            }
            PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead); // KAD SE POPUNI CACHE SALJE SVIMA Dictionary
        }
        public void PopulateConsumptionForecast(NetworkModelTransfer networkModel)
        {
            ConsumptionCalculator consumptionCalculator = new ConsumptionCalculator(networkModel);
            consumptionCalculator.Calculate(productionCached);
            PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
        }
        public void PopulateFlexibility(NetworkModelTransfer networkModel)
        {
            DERFlexibility flexibility = new DERFlexibility(networkModel);
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        if (flexibility.CheckFlexibility(gr.GlobalId))
                        {
                            flexibility.TurnOnFlexibility(10, productionCached, gr.GlobalId);
                        }
                    }
                }
            }
        }

        public void CalculateNewFlexibility(DataToUI data)
        {
            Dictionary<DMSType, long> affectedDERForcast = new Dictionary<DMSType, long>();
            DERFlexibility flexibility = new DERFlexibility();
            string type = "empty";

            if (nmsCache.ContainsKey(data.Gid))
            {
                type = nmsCache[data.Gid].GetType().Name;
            }
            else
            {
                type = "NetworkModel";
            }

            Dictionary<long, IdentifiedObject> affectedEntities = new Dictionary<long, IdentifiedObject>();
            listOfGeneratorsForScada = new Dictionary<long, double>();
            DataToUI dataForScada = new DataToUI();

            copyOfProductionCached = new Dictionary<long, DerForecastDayAhead>(productionCached.Count); // TRENUTNA PROIZVODNJA 24 CASA UNAPRED

            foreach (DerForecastDayAhead der in productionCached.Values)
            {
                copyOfProductionCached.Add(der.entityGid, new DerForecastDayAhead(der));
            }

            if (flexibility.CheckFlexibilityForManualCommanding(data.Gid, nmsCache))
            {
                if (type.Equals("Generator"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];
                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    if (substation.Equipments.Contains(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Generator
                                    {
                                        if (nmsCache[data.Gid].GetType().Name.Equals("Generator"))
                                        {
                                            Generator generator = (Generator)nmsCache[data.Gid];

                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                affectedEntities.Add(gr.GlobalId, gr);

                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                affectedEntities.Add(substation.GlobalId, substation);

                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                affectedEntities.Add(generator.GlobalId, generator);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForGenerator(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForGenerator(data.Flexibility, data.Gid, affectedEntities);
                }
                else if (type.Equals("Substation"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];
                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];
                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    if (substation.GlobalId.Equals(data.Gid))  // TREBA IMPLEMENTIRATI U IFU PROSLEDJIVANJE REGIONA I SUBREGIONA U KOM SE NALAZI Substation
                                    {
                                        foreach (long gen in substation.Equipments)
                                        {
                                            if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)nmsCache[gen];

                                                if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                    affectedEntities.Add(gr.GlobalId, gr);

                                                if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                    affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                                if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                    affectedEntities.Add(substation.GlobalId, substation);

                                                if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                    affectedEntities.Add(generator.GlobalId, generator);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForSubstation(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForSubstation(data.Flexibility, data.Gid, affectedEntities);
                }
                else if (type.Equals("SubGeographicalRegion"))
                {
                    foreach (IdentifiedObject io in nmsCache.Values)
                    {
                        if (io.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)nmsCache[io.GlobalId];

                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                if (subGeographicalRegion.GlobalId.Equals(data.Gid))
                                {

                                    foreach (long sub in subGeographicalRegion.Substations)
                                    {
                                        Substation substation = (Substation)nmsCache[sub];

                                        foreach (long gen in substation.Equipments)
                                        {
                                            if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                            {
                                                Generator generator = (Generator)nmsCache[gen];

                                                if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                    affectedEntities.Add(gr.GlobalId, gr);

                                                if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                    affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                                if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                    affectedEntities.Add(substation.GlobalId, substation);

                                                if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                    affectedEntities.Add(generator.GlobalId, generator);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForSubGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForSubGeoRegion(data.Flexibility, data.Gid, affectedEntities);

                }
                else if (type.Equals("GeographicalRegion"))
                {

                    GeographicalRegion gr = (GeographicalRegion)nmsCache[data.Gid];

                    foreach (long s in gr.Regions)
                    {
                        SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                        foreach (long sub in subGeographicalRegion.Substations)
                        {
                            Substation substation = (Substation)nmsCache[sub];

                            foreach (long gen in substation.Equipments)
                            {
                                if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                {
                                    Generator generator = (Generator)nmsCache[gen];

                                    if (!affectedEntities.ContainsKey(gr.GlobalId))
                                        affectedEntities.Add(gr.GlobalId, gr);

                                    if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                        affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                    if (!affectedEntities.ContainsKey(substation.GlobalId))
                                        affectedEntities.Add(substation.GlobalId, substation);

                                    if (!affectedEntities.ContainsKey(generator.GlobalId))
                                        affectedEntities.Add(generator.GlobalId, generator);
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForGeoRegion(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForGeoRegion(data.Flexibility, data.Gid, affectedEntities);

                }
                else if (type.Equals("NetworkModel"))
                {
                    foreach (var grType in nmsCache.Values)
                    {
                        if (grType.GetType().Name.Equals("GeographicalRegion"))
                        {
                            GeographicalRegion gr = (GeographicalRegion)grType;

                            foreach (long s in gr.Regions)
                            {
                                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)nmsCache[s];

                                foreach (long sub in subGeographicalRegion.Substations)
                                {
                                    Substation substation = (Substation)nmsCache[sub];

                                    foreach (long gen in substation.Equipments)
                                    {
                                        if (nmsCache[gen].GetType().Name.Equals("Generator"))
                                        {
                                            Generator generator = (Generator)nmsCache[gen];

                                            if (!affectedEntities.ContainsKey(gr.GlobalId))
                                                affectedEntities.Add(gr.GlobalId, gr);

                                            if (!affectedEntities.ContainsKey(subGeographicalRegion.GlobalId))
                                                affectedEntities.Add(subGeographicalRegion.GlobalId, subGeographicalRegion);

                                            if (!affectedEntities.ContainsKey(substation.GlobalId))
                                                affectedEntities.Add(substation.GlobalId, substation);

                                            if (!affectedEntities.ContainsKey(generator.GlobalId))
                                                affectedEntities.Add(generator.GlobalId, generator);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    flexibility.CalculateNewDerForecastDayAheadForNetworkModel(data.Flexibility, copyOfProductionCached, data.Gid, affectedEntities);
                    listOfGeneratorsForScada = flexibility.TurnOnFlexibilityForNetworkModel(data.Flexibility, data.Gid, affectedEntities);
                }
            }

            //dataForScada.DataFromCEToScada = listOfGeneratorsForScada;
            //PubSubCalculatioEngine.Instance.Notify(dataForScada, (int)Enums.Topics.Flexibility);

            ClientSideCE.Instance.ProxyScadaListOfGenerators.SendListOfGenerators(listOfGeneratorsForScada);
			ApplyChangesOnProductionCached(); // OVU LINIJU OBRISATI I POZVATI JE KAD SKADA POSALJE ODGOVOR		
		}

        public void ApplyChangesOnProductionCached() // KAD STIGNE POTVRDA SA SKADE DA SU PROMENE IZVRSENE, POZIVAMO OVU METODU KAKO BI NOVI PRORACUNI PROIZVODNJE ZA 24h BILI PRIMENJENI NA CACHE
        {
            productionCached = new Dictionary<long, DerForecastDayAhead>(copyOfProductionCached.Count);

            foreach (DerForecastDayAhead der in copyOfProductionCached.Values)
            {
                productionCached.Add(der.entityGid, new DerForecastDayAhead(der));
            }

            SendDerForecastDayAhead();
            UpdateMinAndMaxFlexibilityForChangedGenerators();
        }

        public void SendDerForecastDayAhead()
        {
            PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
        }

        public Dictionary<long, IdentifiedObject> GetNMSModel()
        {
            return nmsCache;
        }
        public float PopulateBalance(long gid)
        {
            CEUpdateThroughUI ce = new CEUpdateThroughUI();
            float energyFromSource = ce.Balance(productionCached, gid);
            PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
            return energyFromSource;
        }
        public void NetworkModelBalanced()
        {
            PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
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
                PubSubCalculatioEngine.Instance.Notify(CreateDataForUI(), (int)Enums.Topics.DerForecastDayAhead);
        }
        public DataToUI CreateDataForUI()
        {
            DataToUI data = new DataToUI();
            data.Data = productionCached;
            return data;
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



        #region Build Tree
        public void PopulateGraphCached(NetworkModelTransfer networkModelTransfer)
        {
            NetworkModelTreeClass = new List<NetworkModelTreeClass>();
            NetworkModelTreeClass.Add(new NetworkModelTreeClass("Network Model", -1, DMSType.MASK_TYPE, -1, -1));

            if (networkModelTransfer.Insert.Count == 0)
            {
                return;
            }

            if (graphCached == null)
            {
                graphCached = new TreeNode<NodeData>(new NodeData(new IdentifiedObject(-1), true));
            }

            //DO INSERT FIRST TIME 

            //PRVI RED
            foreach (IdentifiedObject idObj in networkModelTransfer.Insert[DMSType.GEOGRAPHICALREGION].Values.ToList())
            {
                NetworkModelTreeClass[0].GeographicalRegions.Add(new GeographicalRegionTreeClass(idObj.Name, idObj.GlobalId, DMSType.GEOGRAPHICALREGION, -1, -1));
                graphCached.AddChild(new NodeData(idObj, DMSType.GEOGRAPHICALREGION, false));
            }

            foreach (IdentifiedObject idOb in networkModelTransfer.Insert[DMSType.GEOGRAPHICALREGION].Values.ToList())
            {

                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == idOb.GlobalId);
                GeographicalRegion geographicalRegion = (GeographicalRegion)found.Data.IdentifiedObject;
                foreach (long gid in geographicalRegion.Regions)
                {
                    IdentifiedObject subRegion = networkModelTransfer.Insert[DMSType.SUBGEOGRAPHICALREGION].Values.ToList().Where(x => x.GlobalId == gid).First();

                    NetworkModelTreeClass[0].GeographicalRegions.Where(x => x.GID == idOb.GlobalId).First()
                                            .GeographicalSubRegions.Add(new GeographicalSubRegionTreeClass(subRegion.Name, subRegion.GlobalId, DMSType.SUBGEOGRAPHICALREGION, -1, -1));

                    found.AddChild(new NodeData(subRegion, DMSType.SUBGEOGRAPHICALREGION, false));
                }
            }

            foreach (IdentifiedObject idOb in networkModelTransfer.Insert[DMSType.SUBGEOGRAPHICALREGION].Values.ToList())
            {
                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == idOb.GlobalId);
                SubGeographicalRegion subRegion = (SubGeographicalRegion)found.Data.IdentifiedObject;
                foreach (long gid in subRegion.Substations)
                {
                    IdentifiedObject substation = networkModelTransfer.Insert[DMSType.SUBSTATION].Values.ToList().Where(x => x.GlobalId == gid).First();

                    GeographicalSubRegionTreeClass subRegionTreeClass = NetworkModelTreeClass[0].GeographicalRegions.SelectMany(x => x.GeographicalSubRegions.Where(y => y.GID == idOb.GlobalId)).FirstOrDefault();
                    subRegionTreeClass.Substations.Add(new SubstationTreeClass(substation.Name, substation.GlobalId, DMSType.SUBSTATION, -1, -1));

                    found.AddChild(new NodeData(substation, DMSType.SUBSTATION, false));
                }
            }
            // 1. 
            foreach (IdentifiedObject idOb in networkModelTransfer.Insert[DMSType.SUBSTATION].Values.ToList())
            {
                SubstationTreeClass substationTreeClass = NetworkModelTreeClass[0].GeographicalRegions.SelectMany(x => x.GeographicalSubRegions.SelectMany(y => y.Substations.Where(z => z.GID == idOb.GlobalId))).FirstOrDefault();

                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == idOb.GlobalId);
                Substation substation = (Substation)found.Data.IdentifiedObject;

                // Nadji eng,source nakacen na taj substaion
                List<EnergySource> energySourcesList = networkModelTransfer.Insert[DMSType.ENEGRYSOURCE].Values.ToList().Cast<EnergySource>().ToList();
                List<EnergySource> energySourcesOfSubstation = energySourcesList.Where(x => x.Container == substation.GlobalId).ToList();

                if (energySourcesOfSubstation != null && energySourcesOfSubstation.Count != 0)
                {
                    foreach (EnergySource es in energySourcesOfSubstation)
                    {
                        //Mozda bude problem jer sam umesto identified obj dala ceo energy source CHECK LATER 
                        found.AddChild(new NodeData(es, DMSType.ENEGRYSOURCE, false));
                        //get terminale nakacene na taj energy source
                        List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList();
                        //Cond eq je u stvari eSRC
                        //ESrc moze da ima vise terminal zato 
                        List<Terminal> terminalOfEnergySrc = terminals.Where(x => x.CondEq == es.GlobalId).ToList();
                        //
                        if (terminalOfEnergySrc != null && terminalOfEnergySrc.Count != 0)
                        {
                            foreach (Terminal terminal in terminalOfEnergySrc)
                            {
                                // Nadjemo cvor na koji je es nakacem i dodamo child
                                TreeNode<NodeData> energySrcConnectedToTerminalFound = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == es.GlobalId);
                                //energySrcConnectedToTerminalFound.AddChild(new NodeData(terminal, DMSType.TERMINAL, false));

                                // U stvari funkcija 
                                DoStartTerminal(terminal, networkModelTransfer, substationTreeClass);
                            }
                        }

                    }
                }
            }

            foreach (Discrete discrete in networkModelTransfer.Insert[DMSType.DISCRETE].Values.ToList())
            {
                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == discrete.PowerSystemResource);

                if (found != null)
                {
                    found.AddChild(new NodeData(discrete, DMSType.DISCRETE, false));
                }
            }

            foreach (Analog analog in networkModelTransfer.Insert[DMSType.ANALOG].Values.ToList())
            {
                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == analog.PowerSystemResource);

                if (found != null)
                {
                    found.AddChild(new NodeData(analog, DMSType.ANALOG, false));
                }
            }

            //CAKI 0314
            foreach (ACLineSegment line in networkModelTransfer.Insert[DMSType.ACLINESEGMENT].Values.ToList())
            {
                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == line.GlobalId);

                if (found != null)
                {
                    foreach (long pointGid in ((ACLineSegment)found.Data.IdentifiedObject).Points)
                    {
                        Point point = (Point)networkModelTransfer.Insert[DMSType.POINT].Values.ToList().Where(x => x.GlobalId == pointGid).First();

                        if (point != null)
                        {
                            found.AddChild(new NodeData(point, DMSType.POINT, false));
                        }
                    }
                }
            }
            //

            ColorGraph();
            CalculateFlexibility();
            //OBAVESTI UI DA JE DOSLO DO PROMENE I POSALJI OVAJ GRAPH
        }

		public void UpdateMinAndMaxFlexibilityForChangedGenerators()
		{
			double minProd = 0;
			double maxProd = 0;
			double currentProd = 0;

			foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModelTreeClass)
			{
				foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
				{
					foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
					{
						foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
						{
							foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
							{
								if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
								{
									if (listOfGeneratorsForScada.ContainsKey(substationElementTreeClass.GID))
									{
										maxProd = substationElementTreeClass.P + substationElementTreeClass.P * (substationElementTreeClass.MaxFlexibility / 100);
										minProd = substationElementTreeClass.P - substationElementTreeClass.P * (substationElementTreeClass.MinFlexibility / 100);

										currentProd = substationElementTreeClass.P + substationElementTreeClass.P * (listOfGeneratorsForScada[substationElementTreeClass.GID] / 100);

										substationElementTreeClass.P = (float)currentProd;
										substationElementTreeClass.MaxFlexibility = (float)(((maxProd - currentProd) * 100) / currentProd);
										substationElementTreeClass.MinFlexibility = (float)(((currentProd - minProd) * 100) / currentProd);
									}
								}
							}
						}
					}
				}
			}

			CalculateFlexibility();
		}

		private void CalculateFlexibility()
        {
            float minFlexibilitySubstation = 0;
            float maxFlexibilitySubstation = 0;
            float productionSubstation = 0;

            float minFlexibilitySubRegion = 0;
            float maxFlexibilitySubRegion = 0;
            float productionSubRegion = 0;

            float minFlexibilityGeoRegion = 0;
            float maxFlexibilityGeoRegion = 0;
            float productionGeoRegion = 0;

            float minFlexibilityNetworkModel = 0;
            float maxFlexibilityNetworkModel = 0;
            float productionNetworkModel = 0;

            foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModelTreeClass)
            {
                foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
                {
                    foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
                    {
                        foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
                        {
                            foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
                            {
                                if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
                                {
                                    productionSubstation += substationElementTreeClass.P;
                                    minFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MinFlexibility) / 100;
                                    maxFlexibilitySubstation += (substationElementTreeClass.P * substationElementTreeClass.MaxFlexibility) / 100;
                                }
                            }

                            substationTreeClass.MinFlexibility = (100 * minFlexibilitySubstation) / productionSubstation;
                            substationTreeClass.MaxFlexibility = (100 * maxFlexibilitySubstation) / productionSubstation;

                            productionSubRegion += productionSubstation;
                            minFlexibilitySubRegion += (productionSubstation * substationTreeClass.MinFlexibility) / 100;
                            maxFlexibilitySubRegion += (productionSubstation * substationTreeClass.MaxFlexibility) / 100;

                            minFlexibilitySubstation = 0;
                            maxFlexibilitySubstation = 0;
                            productionSubstation = 0;
                        }

                        geographicalSubRegionTreeClass.MinFlexibility = (100 * minFlexibilitySubRegion) / productionSubRegion;
                        geographicalSubRegionTreeClass.MaxFlexibility = (100 * maxFlexibilitySubRegion) / productionSubRegion;

                        productionGeoRegion += productionSubRegion;
                        minFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MinFlexibility) / 100;
                        maxFlexibilityGeoRegion += (productionSubRegion * geographicalSubRegionTreeClass.MaxFlexibility) / 100;

                        minFlexibilitySubRegion = 0;
                        maxFlexibilitySubRegion = 0;
                        productionSubRegion = 0;
                    }

                    geographicalRegionTreeClass.MinFlexibility = (100 * minFlexibilityGeoRegion) / productionGeoRegion;
                    geographicalRegionTreeClass.MaxFlexibility = (100 * maxFlexibilityGeoRegion) / productionGeoRegion;

                    productionNetworkModel += productionGeoRegion;
                    minFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MinFlexibility) / 100;
                    maxFlexibilityNetworkModel += (productionGeoRegion * geographicalRegionTreeClass.MaxFlexibility) / 100;

                    minFlexibilityGeoRegion = 0;
                    maxFlexibilityGeoRegion = 0;
                    productionGeoRegion = 0;

                }

                networkModelTreeClasses.MinFlexibility = (100 * minFlexibilityNetworkModel) / productionNetworkModel;
                networkModelTreeClasses.MaxFlexibility = (100 * maxFlexibilityNetworkModel) / productionNetworkModel;

                minFlexibilityNetworkModel = 0;
                maxFlexibilityNetworkModel = 0;
                productionNetworkModel = 0;
            }

            DataToUI data = new DataToUI();
            data.NetworkModelTreeClass = NetworkModelTreeClass;
            PubSubCalculatioEngine.Instance.Notify(data, (int)Enums.Topics.NetworkModelTreeClass);

        }

        private void DoStartTerminal(Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            // Get energy ESRC
            TreeNode<NodeData> energySrcConnectedToTerminalFound = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.CondEq);
            // Add child to ESRC
            energySrcConnectedToTerminalFound.AddChild(new NodeData(terminal, DMSType.TERMINAL, false));

            List<EnergySource> energySourcesOfTerminal = networkModelTransfer.Insert[DMSType.ENEGRYSOURCE].Values.ToList().Cast<EnergySource>().ToList().Where(x => x.Terminals.Contains(terminal.GlobalId)).ToList();
            if (energySourcesOfTerminal != null && energySourcesOfTerminal.Count != 0)
            {
                DealWithSources(energySourcesOfTerminal, terminal, networkModelTransfer, substationTreeClass);
            }
            // TERMINAL ZNA za conn node 
            // Get CN
            ConnectivityNode connectivityNode = networkModelTransfer.Insert[DMSType.CONNECTIVITYNODE].Values.ToList().Cast<ConnectivityNode>().ToList().Where(x => x.GlobalId == terminal.ConnectivityNode).First();
            // OBRADI CNS 
            DoNode(connectivityNode, networkModelTransfer, substationTreeClass);
        }
        private void DoNode(ConnectivityNode connectivityNode, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            // Terminal za koji je node zakacen
            //List<TreeNode<NodeData>> terminalsOfNode = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == connectivityNode.);
            //Moze da se uzima samo jedan po jedan terminal od conn node
            //Unesi conn node u graf

            foreach (long terminalGid in connectivityNode.Terminals)
            {
                TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminalGid);

                if (foundTerminal != null)
                {
                    foundTerminal.AddChild(new NodeData(connectivityNode, DMSType.CONNECTIVITYNODE, false));
                    continue;
                }
                else //1805 CAKI TRY /// not sure if it can help 
                {
                    //continue;
                }

                //TreeNode<NodeData> foundConnectivityNode = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == connectivityNode.GlobalId);
                Terminal terminal = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.GlobalId == terminalGid).First();

                //Ovo izdvojiti u novu funkciju DoEndTerminal
                //foundConnectivityNode.AddChild(new NodeData(terminal,DMSType.TERMINAL,false));
                DoEndTerminal(terminal, networkModelTransfer, substationTreeClass);

            }
        }
        private void DoEndTerminal(Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            // Dodaj terminal na conn node 
            TreeNode<NodeData> foundConnectivityNode = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.ConnectivityNode);
            foundConnectivityNode.AddChild(new NodeData(terminal, DMSType.TERMINAL, false));

            List<Breaker> breakersOfTerminal = networkModelTransfer.Insert[DMSType.BREAKER].Values.ToList().Cast<Breaker>().ToList().Where(x => x.Terminals.Contains(terminal.GlobalId)).ToList();
            List<EnergyConsumer> consumerSOfTerminal = networkModelTransfer.Insert[DMSType.ENERGYCONSUMER].Values.ToList().Cast<EnergyConsumer>().ToList().Where(x => x.Terminals.Contains(terminal.GlobalId)).ToList();
            List<Generator> generatorsOfTerminal = networkModelTransfer.Insert[DMSType.GENERATOR].Values.ToList().Cast<Generator>().ToList().Where(x => x.Terminals.Contains(terminal.GlobalId)).ToList();
            List<ACLineSegment> aclinesOfTerminal = networkModelTransfer.Insert[DMSType.ACLINESEGMENT].Values.ToList().Cast<ACLineSegment>().ToList().Where(x => x.Terminals.Contains(terminal.GlobalId)).ToList();

            if (breakersOfTerminal != null && breakersOfTerminal.Count != 0)
            {
                DealWithBreakers(breakersOfTerminal, terminal, networkModelTransfer, substationTreeClass);
            }

            if (consumerSOfTerminal != null && consumerSOfTerminal.Count != 0)
            {
                DealWithConsumers(consumerSOfTerminal, terminal, networkModelTransfer, substationTreeClass);
            }

            if (generatorsOfTerminal != null && generatorsOfTerminal.Count != 0)
            {
                DealWithGenerators(generatorsOfTerminal, terminal, networkModelTransfer, substationTreeClass);
            }

            if (aclinesOfTerminal != null && aclinesOfTerminal.Count != 0)
            {
                DealWithACLines(aclinesOfTerminal, terminal, networkModelTransfer, substationTreeClass);
            }
        }

        private void DealWithBreakers(List<Breaker> breakers, Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            //GEt terminal
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (Breaker breaker in breakers)
            {
                if (foundTerminal != null)
                {
                    foundTerminal.AddChild(new NodeData(breaker, DMSType.BREAKER, false));
                }

                //Gledam da li postoji jos neki terminal na koji je ovaj nakacen element 
                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(breaker.GlobalId)).ToList();

                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer, substationTreeClass);
                    }
                }

            }

        }
        private void DealWithConsumers(List<EnergyConsumer> consumers, Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (EnergyConsumer consumer in consumers)
            {
                if (foundTerminal != null)
                {
                    substationTreeClass.SubstationElements.Add(new SubstationElementTreeClass(consumer.Name, consumer.GlobalId, DMSType.ENERGYCONSUMER, consumer.PFixed, -1, -1));
                    foundTerminal.AddChild(new NodeData(consumer, DMSType.ENERGYCONSUMER, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(consumer.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer, substationTreeClass);
                    }
                }
            }
        }
        private void DealWithGenerators(List<Generator> generators, Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (Generator generator in generators)
            {
                if (foundTerminal != null)
                {
                    substationTreeClass.SubstationElements.Add(new SubstationElementTreeClass(generator.Name, generator.GlobalId, DMSType.GENERATOR, generator.ConsiderP, generator.MinFlexibility, generator.MaxFlexibility));
                    foundTerminal.AddChild(new NodeData(generator, DMSType.GENERATOR, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(generator.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer, substationTreeClass);
                    }
                }
            }
        }
        private void DealWithACLines(List<ACLineSegment> acLines, Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (ACLineSegment acLine in acLines)
            {
                if (foundTerminal != null)
                {
                    foundTerminal.AddChild(new NodeData(acLine, DMSType.ACLINESEGMENT, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(acLine.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer, substationTreeClass);
                    }
                }
            }
        }

        private void DealWithSources(List<EnergySource> sources, Terminal terminal, NetworkModelTransfer networkModelTransfer, SubstationTreeClass substationTreeClass)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (EnergySource source in sources)
            {
                if (foundTerminal != null)
                {
                    substationTreeClass.SubstationElements.Add(new SubstationElementTreeClass(source.Name, source.GlobalId, DMSType.ENEGRYSOURCE, source.ActivePower, -1, -1)); //dodali P
                    foundTerminal.AddChild(new NodeData(source, DMSType.ENEGRYSOURCE, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(source.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer, substationTreeClass);
                    }
                }
            }

        }
        private void ColorGraph()
        {
            TreeNode<NodeData> rootNode = graphCached.FindTreeNode(x => x.IsRoot);

            foreach (TreeNode<NodeData> node in rootNode.Children)
            {
                //Color nodes - 1 Pass 
                ColorFromNode(node);
            }

            //Color nodes 2. pass
            SecondColoringPass();

        }
        private void ColorFromNode(TreeNode<NodeData> node)
        {
            if (node.Data.Type == DMSType.SUBSTATION ||
                node.Data.Type == DMSType.GEOGRAPHICALREGION || node.Data.Type == DMSType.SUBGEOGRAPHICALREGION || node.Data.Type == DMSType.SUBSTATION ||
                node.Data.Type == DMSType.DISCRETE || node.Data.Type == DMSType.ANALOG)
            {
                foreach (TreeNode<NodeData> child in node.Children)
                {
                    ColorFromNode(child);
                }
            }


            if (node.Data.Type == DMSType.ENEGRYSOURCE)
            {
                node.Data.Energized = Enums.Energized.FromEnergySRC;
            }
            else if (node.Data.Type == DMSType.GENERATOR)
            {
                //CAKI
                TreeNode<NodeData> analog = graphCached.FindTreeNode(x => x.Data.Type == DMSType.ANALOG && ((Analog)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);
                if (analog != null)
                {
                    if (((Analog)analog.Data.IdentifiedObject).NormalValue > 0)
                        node.Data.Energized = Enums.Energized.FromIsland;
                    else
                        node.Data.Energized = Enums.Energized.NotEnergized;
                }
                else
                {
                    node.Data.Energized = Enums.Energized.NotEnergized;
                }

            }


            if (node.Data.Type != DMSType.ENEGRYSOURCE && node.Data.Type != DMSType.BREAKER && node.Data.Type != DMSType.GENERATOR)
            {
                node.Data.Energized = node.Parent.Data.Energized;
            }

            if (node.Data.Type == DMSType.BREAKER)
            {
                //CAKI
                //Posmatramo vr dig signala koji je zakacen za breaker 
                //1705
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                TreeNode<NodeData> digital = graphCached.FindTreeNode(x => x.Data.Type == DMSType.DISCRETE && ((Discrete)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);
                Discrete disc = (Discrete)digital.Data.IdentifiedObject;

                if (disc != null)
                {
                    if (disc.NormalValue == 0)
                    {
                        node.Data.Energized = Enums.Energized.NotEnergized;
                    }
                    else
                    {
                        // 1705
                        node.Data.Energized = node.Parent.Data.Energized;
                    }
                }
                else
                {
                    node.Data.Energized = Enums.Energized.NotEnergized;
                }
                /*Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                if (breaker.NormalOpen)
                {
                    node.Data.Energized = Enums.Energized.NotEnergized;
                }
                else
                {
                    node.Data.Energized = node.Parent.Data.Energized;
                }*/
            }

            foreach (TreeNode<NodeData> child in node.Children)
            {
                ColorFromNode(child);
            }

        }
        private void SecondColoringPass()
        {
            TreeNode<NodeData> rootNode = graphCached.FindTreeNode(x => x.IsRoot);
            foreach (TreeNode<NodeData> node in rootNode.Children)
            {
                foundSinchronousMachine(node);
            }

        }
        private void ColorFromBottom(TreeNode<NodeData> node)
        {
            if (node.Data.Type == DMSType.GENERATOR)
            {
                if (node.Data.Energized == Enums.Energized.FromEnergySRC)
                {
                    return;
                }
                node.Data.Energized = Enums.Energized.FromIsland;
                ColorFromBottom(node.Parent);
            }
            else if (node.Data.Type == DMSType.BREAKER)
            {
                //1705
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                TreeNode<NodeData> digital = graphCached.FindTreeNode(x => x.Data.Type == DMSType.DISCRETE && ((Discrete)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);
                Discrete disc = (Discrete)digital.Data.IdentifiedObject;
                if (disc != null)
                {
                    if (disc.NormalValue == 0) // otvoren nema nista 
                    {

                    }
                    else // zatvoren - napajanje moze da prodje 
                    {
                        if (node.Data.Energized == Enums.Energized.FromEnergySRC)
                        {
                            // Go down 
                            ColorCorrection(node);
                        }
                        else if (node.Data.Energized == Enums.Energized.FromIsland)
                        {
                            // do nothing 
                        }
                        else if (node.Data.Energized == Enums.Energized.NotEnergized)
                        {
                            node.Data.Energized = Enums.Energized.FromIsland;
                            ColorFromBottom(node.Parent);
                        }
                        //node.Data.Energized = node.Parent.Data.Energized;
                    }
                }
                else
                {
                    node.Data.Energized = Enums.Energized.NotEnergized;
                }


                //1705

                //vidi stanje ali u sustini se vrati nema zasto gore da ide
                /*Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                if (!breaker.NormalOpen)
                {
                    if (node.Data.Energized == Enums.Energized.FromEnergySRC)
                    {
                        // Go down 
                        ColorCorrection(node);
                    }
                    else if (node.Data.Energized == Enums.Energized.FromIsland)
                    {
                        // do nothing 
                    }
                    else if (node.Data.Energized == Enums.Energized.NotEnergized)
                    {
                        node.Data.Energized = Enums.Energized.FromIsland;
                        ColorFromBottom(node.Parent);
                    }
                    //node.Data.Energized = node.Parent.Data.Energized;

                }*/
                return;
            }
            else
            {
                // Energizuj ga kao sto mu je child energizovan
                if (node.Data.Energized == Enums.Energized.NotEnergized)
                {
                    node.Data.Energized = Enums.Energized.FromIsland;
                    ColorFromBottom(node.Parent);
                    foreach (TreeNode<NodeData> children in node.Children)
                    {
                        ColorChildrenSecondPass(children);
                    }
                }

            }
        }

        //new method 1705
        public void ColorCorrection(TreeNode<NodeData> node)
        {
            foreach (TreeNode<NodeData> child in node.Children)
            {
                ReColor(node);
            }
        }

        //ReColour to green 1705 
        public void ReColor(TreeNode<NodeData> node)   
        {
            node.Data.Energized = Enums.Energized.FromEnergySRC;
        }

        public void ColorChildrenSecondPass(TreeNode<NodeData> node)
        {
            if (node.Data.Energized != Enums.Energized.NotEnergized || node.Data.Type == DMSType.ANALOG || node.Data.Type == DMSType.DISCRETE)
            {
                return;
            }

            if (node.Data.Type == DMSType.BREAKER)
            {
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                TreeNode<NodeData> digital = graphCached.FindTreeNode(x => x.Data.Type == DMSType.DISCRETE && ((Discrete)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);
                Discrete disc = (Discrete)digital.Data.IdentifiedObject;
                if (disc.NormalValue == 0)
                {
                    return;
                }

                node.Data.Energized = Enums.Energized.FromIsland;
                foreach (TreeNode<NodeData> child in node.Children)
                {
                    ColorChildrenSecondPass(child);
                }

            }
            else if (node.Data.Type == DMSType.ENERGYCONSUMER || node.Data.Type == DMSType.GENERATOR)
            {
                node.Data.Energized = Enums.Energized.FromIsland;
                return;
            }
            else
            {
                node.Data.Energized = Enums.Energized.FromIsland;
                foreach (TreeNode<NodeData> child in node.Children)
                {
                    ColorChildrenSecondPass(child);
                }
            }
        }
        private void foundSinchronousMachine(TreeNode<NodeData> node)
        {
            if (node.Data.Type == DMSType.GENERATOR)
            {
                ColorFromBottom(node);
            }

            foreach (TreeNode<NodeData> child in node.Children)
            {
                foundSinchronousMachine(child);
            }
        }
        #endregion

        #region Update Tree From SCADA
        public void UpdateGraphWithScadaValues(List<DataPoint> data)
        {
            if (graphCached == null)
                return;

            foreach (DataPoint dp in data)
            {
                TreeNode<NodeData> node = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == dp.Gid);
                if (node == null)
                    continue;

                if (node.Data.Type == DMSType.ANALOG)
                {
                    //node.Data.Value = float.Parse(dp.Value, CultureInfo.InvariantCulture.NumberFormat);
                    //
                    ((Analog)node.Data.IdentifiedObject).NormalValue = float.Parse(dp.Value, CultureInfo.InvariantCulture.NumberFormat);
                }
                else if (node.Data.Type == DMSType.DISCRETE)
                {
                    int vrednost;
                    if (dp.Value == "OFF")
                        vrednost = 0;
                    else vrednost = 1;
                    ((Discrete)node.Data.IdentifiedObject).NormalValue = vrednost;
                }
            }

            ColorGraph();
        }
        #endregion
    }
}
