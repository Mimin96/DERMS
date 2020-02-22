using DarkSkyApi.Models;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
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

        public TreeNode<NodeData> GraphCached 
        {
            get { return graphCached; }
            set { graphCached = value; }
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
                        ToLatLon(gr.Latitude, gr.Longitude, 34, out lat, out lon);
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(lat, lon).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("Generator"))
                    {
                        var gr = (Generator)kvpDic.Value;
                        ToLatLon(gr.Latitude, gr.Longitude, 34, out lat, out lon);
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(lat, lon).Result, kvpDic.Key);
                    }
                    else if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
                        ToLatLon(gr.Latitude, gr.Longitude, 34, out lat, out lon);
                        AddForecast(darkSkyApi.GetWeatherForecastAsync(lat, lon).Result, kvpDic.Key);
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

        public void PopulateGraphCached(NetworkModelTransfer networkModelTransfer)
        {
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
                graphCached.AddChild(new NodeData(idObj, DMSType.GEOGRAPHICALREGION, false));
            }

            foreach (IdentifiedObject idOb in networkModelTransfer.Insert[DMSType.GEOGRAPHICALREGION].Values.ToList())
            {

                TreeNode<NodeData> found = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == idOb.GlobalId);
                GeographicalRegion geographicalRegion = (GeographicalRegion)found.Data.IdentifiedObject;
                foreach (long gid in geographicalRegion.Regions)
                {
                    IdentifiedObject subRegion = networkModelTransfer.Insert[DMSType.SUBGEOGRAPHICALREGION].Values.ToList().Where(x => x.GlobalId == gid).First();
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
                    found.AddChild(new NodeData(substation, DMSType.SUBSTATION, false));
                }
            }
            // 1. 
            foreach (IdentifiedObject idOb in networkModelTransfer.Insert[DMSType.SUBSTATION].Values.ToList())
            {
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
                                DoStartTerminal(terminal, networkModelTransfer);
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


            ColorGraph();

            //OBAVESTI UI DA JE DOSLO DO PROMENE I POSALJI OVAJ GRAPH
        }

        private void DoStartTerminal(Terminal terminal, NetworkModelTransfer networkModelTransfer)
        {
            // Get energy ESRC
            TreeNode<NodeData> energySrcConnectedToTerminalFound = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.CondEq);
            // Add child to ESRC
            energySrcConnectedToTerminalFound.AddChild(new NodeData(terminal, DMSType.TERMINAL, false));
            // TERMINAL ZNA za conn node 
            // Get CN
            ConnectivityNode connectivityNode = networkModelTransfer.Insert[DMSType.CONNECTIVITYNODE].Values.ToList().Cast<ConnectivityNode>().ToList().Where(x => x.GlobalId == terminal.ConnectivityNode).First();
            // OBRADI CNS 
            DoNode(connectivityNode, networkModelTransfer);
        }

        private void DoNode(ConnectivityNode connectivityNode, NetworkModelTransfer networkModelTransfer)
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

                //TreeNode<NodeData> foundConnectivityNode = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == connectivityNode.GlobalId);
                Terminal terminal = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.GlobalId == terminalGid).First();

                //Ovo izdvojiti u novu funkciju DoEndTerminal
                //foundConnectivityNode.AddChild(new NodeData(terminal,DMSType.TERMINAL,false));
                DoEndTerminal(terminal, networkModelTransfer);

            }
        }

        private void DoEndTerminal(Terminal terminal, NetworkModelTransfer networkModelTransfer)
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
                DealWithBreakers(breakersOfTerminal, terminal, networkModelTransfer);
            }

            if (consumerSOfTerminal != null && consumerSOfTerminal.Count != 0)
            {
                DealWithConsumers(consumerSOfTerminal, terminal, networkModelTransfer);
            }

            if (generatorsOfTerminal != null && generatorsOfTerminal.Count != 0)
            {
                DealWithGenerators(generatorsOfTerminal, terminal, networkModelTransfer);
            }

            if (aclinesOfTerminal != null && aclinesOfTerminal.Count != 0)
            {
                DealWithACLines(aclinesOfTerminal, terminal, networkModelTransfer);
            }

        }

        private void DealWithBreakers(List<Breaker> breakers, Terminal terminal, NetworkModelTransfer networkModelTransfer)
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
                        DoStartTerminal(t, networkModelTransfer);
                    }
                }

            }

        }

        private void DealWithConsumers(List<EnergyConsumer> consumers, Terminal terminal, NetworkModelTransfer networkModelTransfer)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (EnergyConsumer consumer in consumers)
            {
                if (foundTerminal != null)
                {
                    foundTerminal.AddChild(new NodeData(consumer, DMSType.ENERGYCONSUMER, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(consumer.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer);
                    }
                }
            }
        }

        private void DealWithGenerators(List<Generator> generators, Terminal terminal, NetworkModelTransfer networkModelTransfer)
        {
            TreeNode<NodeData> foundTerminal = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == terminal.GlobalId);

            foreach (Generator generator in generators)
            {
                if (foundTerminal != null)
                {
                    foundTerminal.AddChild(new NodeData(generator, DMSType.GENERATOR, false));
                }

                List<Terminal> terminals = networkModelTransfer.Insert[DMSType.TERMINAL].Values.ToList().Cast<Terminal>().ToList().Where(x => x.CondEq.Equals(generator.GlobalId)).ToList();
                if (terminals != null && terminals.Count != 0)
                {
                    foreach (Terminal t in terminals)
                    {
                        if (terminal.GlobalId == t.GlobalId)
                            continue;
                        DoStartTerminal(t, networkModelTransfer);
                    }
                }
            }
        }

        private void DealWithACLines(List<ACLineSegment> acLines, Terminal terminal, NetworkModelTransfer networkModelTransfer)
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
                        DoStartTerminal(t, networkModelTransfer);
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
                TreeNode<NodeData> analog = graphCached.FindTreeNode(x=>x.Data.Type == DMSType.ANALOG && ((Analog)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);
                if (analog != null)
                {
                    if(analog.Data.Value > 0)
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
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                TreeNode<NodeData> digital = graphCached.FindTreeNode(x => x.Data.Type == DMSType.DISCRETE && ((Discrete)x.Data.IdentifiedObject).PowerSystemResource == node.Data.IdentifiedObject.GlobalId);

                if (digital != null)
                {
                    if (digital.Data.Value == 0)
                    {
                        node.Data.Energized = Enums.Energized.NotEnergized;
                    }
                    else
                    {
                        node.Data.Energized = node.Parent.Data.Energized;
                    }
                }
                else
                {
                    node.Data.Energized = Enums.Energized.NotEnergized;
                }

                /*if (breaker.NormalOpen)
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
                //vidi stanje ali u sustini se vrati nema zasto gore da ide
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                if (!breaker.NormalOpen)
                {
                    if (node.Data.Energized == Enums.Energized.NotEnergized)
                    {
                        node.Data.Energized = Enums.Energized.FromIsland;
                        ColorFromBottom(node.Parent);
                    }
                }
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

        public void ColorChildrenSecondPass(TreeNode<NodeData> node)
        {
            if (node.Data.Energized != Enums.Energized.NotEnergized || node.Data.Type == DMSType.ANALOG || node.Data.Type == DMSType.DISCRETE)
            {
                return;
            }

            if (node.Data.Type == DMSType.BREAKER)
            {
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                if (breaker.NormalOpen)
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

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        //CAKI 2102
        public void UpdateGraphWithScadaValues(List<DataPoint> data)
        {
            if (graphCached == null)
                return;

            foreach(DataPoint dp in data)
            {
                TreeNode<NodeData> node = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == dp.Gid);
                if (node == null)
                    continue;

                if (node.Data.Type == DMSType.ANALOG)
                {
                    node.Data.Value = float.Parse(dp.Value, CultureInfo.InvariantCulture.NumberFormat);
                }
                else if (node.Data.Type == DMSType.DISCRETE)
                {
                    node.Data.Value = int.Parse(dp.Value, CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            ColorGraph();
        }



    }
}
