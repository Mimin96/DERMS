using CloudCommon.CalculateEngine;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.NMSCommuication;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeConstructionMicroservice
{
    public class TreeConstructionService : ITreeConstruction
    {
        // Treba da se proveri pre poziva ove metode da li postoji odr

        private List<NetworkModelTreeClass> NetworkModelTreeClass;
        private TreeNode<NodeData> graphCached;

        public TreeConstructionService()
        {
            NetworkModelTreeClass = new List<NetworkModelTreeClass>();
        }

        public TreeNode<NodeData> GraphCached
        {
            get { return graphCached; }
            set { graphCached = value; }
        }

        public async Task<TreeNode<NodeData>> ConstructTree1(NetworkModelTransfer networkModelTransfer)
        {
            PopulateGraphCached(networkModelTransfer);
            return GraphCached;
        }

        //Should be used for first construction of the tree
        //public TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer)
        //{
        //    PopulateGraphCached(networkModelTransfer);
        //    return GraphCached;
        //}

        //Should be used for existing tree
        public TreeNode<NodeData> ConstructTree(NetworkModelTransfer networkModelTransfer, TreeNode<NodeData> graphCached)
        {
            this.GraphCached = graphCached;
            PopulateGraphCached(networkModelTransfer);
            return GraphCached;
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
            //CalculateFlexibility();
            //OBAVESTI UI DA JE DOSLO DO PROMENE I POSALJI OVAJ GRAPH
        }

        public async Task<List<NetworkModelTreeClass>> GetNetworkModelTreeClass()
		{
            return NetworkModelTreeClass;
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
                    if (disc.NormalValue == 1)
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
                    if (disc.NormalValue == 1) // otvoren nema nista 
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
                if (disc.NormalValue == 1)
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
        public async Task<TreeNode<NodeData>> UpdateGraphWithScadaValues(List<DataPoint> data, TreeNode<NodeData> rcvgraphCached)
        {
            if (rcvgraphCached == null)
                return null;
            GraphCached = rcvgraphCached;

            foreach (DataPoint dp in data)
            {
                TreeNode<NodeData> node = graphCached.FindTreeNode(x => x.Data.IdentifiedObject.GlobalId == dp.Gid);
                if (node == null)
                    continue;

                if (node.Data.Type == DMSType.ANALOG)
                {
                    //node.Data.Value = float.Parse(dp.Value, CultureInfo.InvariantCulture.NumberFormat);
                    //
                    ((Analog)node.Data.IdentifiedObject).NormalValue = float.Parse(dp.RawValue.ToString(), CultureInfo.InvariantCulture.NumberFormat);
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
            return GraphCached;
        }
        #endregion
    }
}
