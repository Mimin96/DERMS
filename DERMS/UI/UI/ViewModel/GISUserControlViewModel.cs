using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Resources;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Documents;
using DERMSCommon.UIModel;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.DataModel.Wires;
using FTN.Common;
using UI.View;
using UI.Resources.MediatorPattern;
using DERMSCommon.UIModel.ThreeViewModel;
using UI.Communication;

namespace UI.ViewModel
{
    public class GISUserControlViewModel : BindableBase
    {
        #region Variables
        private TreeNode<NodeData> _tree;
        private TextBox _gisTextBlock;
        private Map _map;
        private Dictionary<string, bool> _visibilityOfElements;
        private RelayCommand<object> _searchCommand;
        private string _searchParam;
        CommunicationProxy proxy;
        private List<Generator> TurnedOffGenerators { get; set; }
        #endregion

        public GISUserControlViewModel(Map map, TextBox gisTextBlock)
        {
            Mediator.Register("NMSNetworkModelDataGIS", NMSNetworkModelDataGIS);
            TurnedOffGenerators = new List<Generator>();

            VisibilityOfElements = new Dictionary<string, bool>();
            VisibilityOfElementPopulate();
            SearchParameter = "Element Name";

            _map = map;
            _gisTextBlock = gisTextBlock;

            _map.ZoomLevel = 12;
            _map.Center = new Location(45.27143, 19.7794009);
        }

        #region Properties
        public TreeNode<NodeData> Tree
        {
            get
            {
                return _tree;
            }
            set
            {
                _tree = value;
                SetTreeOnMap();
            }
        }

        public Dictionary<string, bool> VisibilityOfElements
        {
            get
            {
                return _visibilityOfElements;
            }
            set
            {
                _visibilityOfElements = value;
            }
        }

        public string SearchParameter
        {
            get
            {
                return _searchParam;
            }
            set
            {
                _searchParam = value;
                OnPropertyChanged("SearchParameter");
            }
        }
		#endregion

		#region Public Methods

		public void OnFocusSearchParameter(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Element Name")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusSearchParameter(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Element Name")
            {
                textBox.Text = "Element Name";
                SearchParameter = "Element Name";
                textBox.FontStyle = FontStyles.Italic;
            }
        }

        public void NMSNetworkModelDataGIS(object parameter) 
        {
            List<object> obj = (List<object>)parameter;
            Tree = (TreeNode<NodeData>)obj[0];
        }
        public void GetCoordinatesOnMouseClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            UIElement iElement = null;
            UIElementCollection ee = ((Map)sender).Children;

            foreach (UIElement uIElement in ee)
            {
                if (uIElement.IsMouseOver)
                {
                    iElement = uIElement;
                    break;
                }
            }

            if (iElement == null)
                return;

            TreeNode<NodeData> selected = _tree.Where(x => x.Data.IdentifiedObject.GlobalId.ToString() == iElement.Uid).FirstOrDefault();

            if (selected.Data.Type == DMSType.BREAKER)
            {
                Breaker breaker = (Breaker)selected.Data.IdentifiedObject;
                Window window = new BreakerControlThroughGISWindow();
                long GIDm = breaker.Measurements.FirstOrDefault();

                Discrete discrete = (Discrete)Tree.Where(t => t.Data.IdentifiedObject.GlobalId == GIDm).FirstOrDefault().Data.IdentifiedObject;

                if (discrete.NormalValue == 0)
                    ((BreakerControlThroughGISWindowViewModel)window.DataContext).Close = true;
                else
                    ((BreakerControlThroughGISWindowViewModel)window.DataContext).Open = true;

                ((BreakerControlThroughGISWindowViewModel)window.DataContext).GID = breaker.GlobalId;

                window.Show();
            }
            else if (selected.Data.Type == DMSType.GENERATOR)
            {
                bool canManualCommand = true;
                string text = "";
                Generator generator = (Generator)selected.Data.IdentifiedObject;
                proxy = new CommunicationProxy();
                proxy.Open2();
                TurnedOffGenerators = proxy.sendToCE.GeneratorOffCheck();
                foreach (Generator g in TurnedOffGenerators)
                {
                    if (g.GlobalId.Equals(generator.GlobalId))
                    {
                        canManualCommand = false;
                        //return;
                    }
                }
                if (canManualCommand)
                {
                    Window w = new ManualCommandingWindow(generator.MaxFlexibility, generator.MinFlexibility, selected.Data.IdentifiedObject.GlobalId);
                    w.Show();
                }
                else
                {
                    text = "Generator is disconnected from network";
                    PopUpWindow popUpWindow = new PopUpWindow(text);
                    popUpWindow.ShowDialog();
                }
            
            }
        }
        public void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            UIElement iElement = null;
            UIElementCollection ee = ((Map)sender).Children;

            foreach (UIElement uIElement in ee)
            {
                if (uIElement.IsMouseOver)
                {
                    iElement = uIElement;
                    break;
                }
            }

            if (iElement == null)
                return;

            TreeNode<NodeData> selected = _tree.Where(x => x.Data.IdentifiedObject.GlobalId.ToString() == iElement.Uid).FirstOrDefault();

            _gisTextBlock.Text = String.Empty;
            _gisTextBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0398E2"));
            _gisTextBlock.AppendText(BuildToolTipOnClick(selected));
        }
        public void ExecuteVisibilityOfElements(object sender, MouseButtonEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            VisibilityOfElements[checkBox.Name] = ((bool)checkBox.IsChecked) ? false : true;

            SetTreeOnMap();
        }
        #endregion

        #region Private Methods
        private void VisibilityOfElementPopulate()
        {
            VisibilityOfElements["EnergySource"] = true;
            VisibilityOfElements["SolarPanel"] = true;
            VisibilityOfElements["WindTurbine"] = true;
            VisibilityOfElements["EnergyConsumer"] = true;
            VisibilityOfElements["DERBlue"] = true;
            VisibilityOfElements["DERGreen"] = true;
            VisibilityOfElements["DERRed"] = true;
            VisibilityOfElements["LineBlue"] = true;
            VisibilityOfElements["LineGreen"] = true;
            VisibilityOfElements["LineRed"] = true;

            OnPropertyChanged("VisibilityOfElements");
        }
        private string BuildToolTipOnClick(TreeNode<NodeData> selected)
        {
            StringBuilder stringBuilderFinal = new StringBuilder();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("GID: {0}{1}", selected.Data.IdentifiedObject.GlobalId, Environment.NewLine);
            stringBuilder.AppendFormat("Name: {0}{1}", selected.Data.IdentifiedObject.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}{1}", selected.Data.IdentifiedObject.Description, Environment.NewLine);
            stringBuilder.AppendFormat("{0}", Environment.NewLine);
            stringBuilder.AppendFormat("{0}{1}", selected.Data.Type.ToString(), Environment.NewLine);
            stringBuilder.AppendFormat("Energized: {0}{1}", selected.Data.Energized.ToString(), Environment.NewLine);

            long substationGID = 0;
            List<long> measurementGIDs = new List<long>();

            switch (selected.Data.Type)
            {
                case FTN.Common.DMSType.ACLINESEGMENT:
                    ACLineSegment lineSegment = (ACLineSegment)selected.Data.IdentifiedObject;
                    stringBuilder.AppendFormat("Current Flow: {0}{1}", lineSegment.CurrentFlow, Environment.NewLine);
                    stringBuilder.AppendFormat("Type of AC Line Segment: {0}{1}", lineSegment.Type.ToString(), Environment.NewLine);
                    substationGID = lineSegment.Container;
                    measurementGIDs = lineSegment.Measurements;
                    break;
                case FTN.Common.DMSType.BREAKER:
                    Breaker breaker = (Breaker)selected.Data.IdentifiedObject;
                    
                    if (breaker.NormalOpen)
                    {
                        stringBuilder.AppendFormat("Normal open state: true (0) {1}", breaker.NormalOpen.ToString(), Environment.NewLine);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("Normal open state: false (1) {1}", breaker.NormalOpen.ToString(), Environment.NewLine);
                    }
                    substationGID = breaker.Container;
                    measurementGIDs = breaker.Measurements;
                    break;
                case FTN.Common.DMSType.ENEGRYSOURCE:
                    EnergySource energySource = (EnergySource)selected.Data.IdentifiedObject;
                    stringBuilder.AppendFormat("Type: {0}{1}", energySource.Type.ToString(), Environment.NewLine);
                    stringBuilder.AppendFormat("Nominal Voltage: {0}     ", energySource.NominalVoltage);
                    stringBuilder.AppendFormat("Magnitude Voltage: {0}     ", energySource.MagnitudeVoltage);
                    stringBuilder.AppendFormat("Active Power: {0}{1}", energySource.ActivePower, Environment.NewLine);
                    substationGID = energySource.Container;
                    measurementGIDs = energySource.Measurements;
                    break;
                case FTN.Common.DMSType.ENERGYCONSUMER:
                    EnergyConsumer energyConsumer = (EnergyConsumer)selected.Data.IdentifiedObject;
                    stringBuilder.AppendFormat("P Fixed: {0}     ", energyConsumer.PFixed);
                    stringBuilder.AppendFormat("Q Fixed: {0}{1}", energyConsumer.QFixed, Environment.NewLine);
                    substationGID = energyConsumer.Container;
                    measurementGIDs = energyConsumer.Measurements;
                    break;
                case FTN.Common.DMSType.GENERATOR:
                    Generator generator = (Generator)selected.Data.IdentifiedObject;
                    stringBuilder.AppendFormat("Type: {0}{1}", generator.GeneratorType.ToString(), Environment.NewLine);
                    stringBuilder.AppendFormat("Consider P: {0}{1}", generator.ConsiderP, Environment.NewLine);
                    substationGID = generator.Container;
                    measurementGIDs = generator.Measurements;
                    break;
                default:
                    break;
            }

            Substation substation = (Substation)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == substationGID).FirstOrDefault().Data.IdentifiedObject;
            SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == substation.SubGeoReg).FirstOrDefault().Data.IdentifiedObject;
            GeographicalRegion geographicalRegion = (GeographicalRegion)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == subGeographicalRegion.GeoReg).FirstOrDefault().Data.IdentifiedObject;

            stringBuilderFinal.AppendFormat("Geographical Region: {0}     ", geographicalRegion.Name);
            stringBuilderFinal.AppendFormat("SubGeographical Region: {0}     ", subGeographicalRegion.Name);
            stringBuilderFinal.AppendFormat("Substation: {0}{1}", substation.Name, Environment.NewLine);
            stringBuilderFinal.AppendFormat("{0}", Environment.NewLine);
            stringBuilderFinal.Append(stringBuilder.ToString());
            stringBuilderFinal.AppendFormat("{0}Measurements {1}", Environment.NewLine, Environment.NewLine);
            int i = 0;

            if (measurementGIDs.Count == 0)
            {
                stringBuilderFinal.AppendFormat("NaN");
            }

            foreach (long gid in measurementGIDs)
            {
                stringBuilderFinal.AppendFormat("[{0}]{1}", i++, Environment.NewLine);
                TreeNode<NodeData> treeNode = _tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid).FirstOrDefault();

                if (treeNode.Data.Type == FTN.Common.DMSType.ANALOG)
                {
                    Analog analog = (Analog)treeNode.Data.IdentifiedObject;
                    stringBuilderFinal.AppendFormat("Name: {0}{1}", analog.Name, Environment.NewLine);
                    stringBuilderFinal.AppendFormat("Measurement Type: {0}{1}", analog.MeasurementType, Environment.NewLine);
                    stringBuilderFinal.AppendFormat("Min Value: {0}     ", analog.MinValue);
                    stringBuilderFinal.AppendFormat("Max Value: {0}     ", analog.MaxValue);
                    stringBuilderFinal.AppendFormat("Normal Value: {0}{1}", analog.NormalValue, Environment.NewLine);
                }
                else
                {
                    Discrete discrete = (Discrete)treeNode.Data.IdentifiedObject;
                    stringBuilderFinal.AppendFormat("Name: {0}{1}", discrete.Name, Environment.NewLine);
                    stringBuilderFinal.AppendFormat("Measurement Type: {0}{1}", discrete.MeasurementType, Environment.NewLine);
                    stringBuilderFinal.AppendFormat("Min Value: {0}     ", discrete.MinValue);
                    stringBuilderFinal.AppendFormat("Max Value: {0}     ", discrete.MaxValue);
                    if (discrete.NormalValue == 1)
                    {
                        stringBuilderFinal.AppendFormat("Normal Value: OPEN({0}){1}", discrete.NormalValue, Environment.NewLine);
                    }
                    else
                    {
                        stringBuilderFinal.AppendFormat("Normal Value: CLOSED({0}){1}", discrete.NormalValue, Environment.NewLine);
                    }
                }
            }

            return stringBuilderFinal.ToString();
        }
        private void SetTreeOnMap()
        {
            if (_tree == null)
                return;

            _map.Children.Clear();

            List<TreeNode<NodeData>> energySources = _tree.Where(x => x.Data.Type == FTN.Common.DMSType.ENEGRYSOURCE).ToList();

            foreach (TreeNode<NodeData> node in energySources)
            {
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder stringBuilderUniversal = new StringBuilder();
                EnergySource energySource = (EnergySource)node.Data.IdentifiedObject;

                Substation substation = (Substation)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == energySource.Container).FirstOrDefault().Data.IdentifiedObject;
                SubGeographicalRegion subGeographicalRegion = (SubGeographicalRegion)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == substation.SubGeoReg).FirstOrDefault().Data.IdentifiedObject;
                GeographicalRegion geographicalRegion = (GeographicalRegion)_tree.Where(x => x.Data.IdentifiedObject.GlobalId == subGeographicalRegion.GeoReg).FirstOrDefault().Data.IdentifiedObject;

                stringBuilderUniversal.AppendFormat("Geographical Region: {0}{1}", geographicalRegion.Name, Environment.NewLine);
                stringBuilderUniversal.AppendFormat("SubGeographical Region: {0}{1}", subGeographicalRegion.Name, Environment.NewLine);
                stringBuilderUniversal.AppendFormat("Substation: {0}{1}", substation.Name, Environment.NewLine);
                stringBuilderUniversal.AppendFormat("----------------------------------------{0}", Environment.NewLine);

                Location pinLocation = new Location(energySource.Longitude, energySource.Latitude);

                stringBuilder.Append(stringBuilderUniversal.ToString());
                stringBuilder.AppendFormat("Name: {0}{1}", energySource.Name, Environment.NewLine);
                stringBuilder.AppendFormat("Description: {0}{1}", energySource.Description, Environment.NewLine);
                stringBuilder.AppendFormat("Nominal Voltage: {0} kW", energySource.NominalVoltage);
                string toolTip = stringBuilder.ToString();

                Pushpin pushpin = new Pushpin();
                pushpin.Uid = energySource.GlobalId.ToString();
                pushpin.Location = pinLocation;
                pushpin.ToolTip = toolTip;
                pushpin.Cursor = Cursors.Hand;
                pushpin.Template = (ControlTemplate)Application.Current.Resources["EnergySourceTemplate"];

                if (VisibilityOfElements["EnergySource"])
                    _map.Children.Add(pushpin);

                StartDrowingOnMap(node.Children.ToList(), stringBuilderUniversal.ToString());
            }
        }
        private void StartDrowingOnMap(List<TreeNode<NodeData>> elements, string stringBuilderUniversal)
        {
            foreach (TreeNode<NodeData> node in elements)
            {
                switch (node.Data.Type)
                {
                    case FTN.Common.DMSType.ACLINESEGMENT:
                        if (CanDrowAcLine(node.Data.Energized))
                            DrowOnMapAcLineSegment(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.BREAKER:
                        if (CanDrowBreaker(node.Data.Energized))
                            DrowOnMapBreaker(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.ENERGYCONSUMER:
                        if (VisibilityOfElements["EnergyConsumer"])
                            DrowOnMapEnergyConsumer(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.GENERATOR:
                        if (CanDrowGenerator(((Generator)node.Data.IdentifiedObject).GeneratorType))
                            DrowOnMapDER(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.DISCRETE:
                        DrowOnMapDiscrete(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.ANALOG:
                        DrowOnMapAnalog(node, stringBuilderUniversal);
                        break;
                    default:
                        break;
                }

                StartDrowingOnMap(node.Children.ToList(), stringBuilderUniversal);
            }
        }
        //ovu metodu treba jos doraditi kada se uvuce pravilan xml fajl sa vise kordinata za AcLine
        private void DrowOnMapAcLineSegment(TreeNode<NodeData> acLine, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            ACLineSegment acLineSegment = (ACLineSegment)acLine.Data.IdentifiedObject;

            stringBuilder.Append(stringBuilderUniversal);
            stringBuilder.AppendFormat("Name: {0}{1}", acLineSegment.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}{1}", acLineSegment.Description, Environment.NewLine);
            stringBuilder.AppendFormat("Type: {0}", acLineSegment.Type.ToString());
            string toolTip = stringBuilder.ToString();

            Location pinLocation = new Location(acLineSegment.Longitude, acLineSegment.Latitude);

            MapPolygon polygon = new MapPolygon();
            MapPolyline line = new MapPolyline();
            line.ToolTip = toolTip;

            if (acLine.Data.Energized == Enums.Energized.FromEnergySRC)
                line.Stroke = new SolidColorBrush(Colors.Green);
            else if (acLine.Data.Energized == Enums.Energized.FromIsland)
                line.Stroke = new SolidColorBrush(Colors.Blue);
            else
                line.Stroke = new SolidColorBrush(Colors.Red);

            line.Uid = acLineSegment.GlobalId.ToString();
            line.StrokeThickness = 2;
            line.Opacity = 0.9;
            line.Cursor = Cursors.Hand;
            line.Locations = new LocationCollection();


            foreach (long item in acLineSegment.Points)
            {
                TreeNode<NodeData> data = acLine.Children.Where(X => X.Data.IdentifiedObject.GlobalId == item).First();
                DERMSCommon.DataModel.Core.Point point1 = (DERMSCommon.DataModel.Core.Point)(data.Data.IdentifiedObject);
                //polygon.Locations.Add(new Location(point1.Longitude, point1.Latitude));
                line.Locations.Add(new Location(point1.Longitude, point1.Latitude));
            }

            _map.Children.Add(line);
        }

        private void DrowOnMapBreaker(TreeNode<NodeData> breaker, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Breaker breaker1 = (Breaker)breaker.Data.IdentifiedObject;

            Location pinLocation = new Location(breaker1.Longitude, breaker1.Latitude);
            stringBuilder.Append(stringBuilderUniversal);
            stringBuilder.AppendFormat("Name: {0}{1}", breaker1.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}{1}", breaker1.Description, Environment.NewLine);

            if (breaker1.NormalOpen)
            {
                stringBuilder.AppendFormat("Normal Open: true", Environment.NewLine);
            }
            else
            {
                stringBuilder.AppendFormat("Normal Open: false", Environment.NewLine);
            }

            string toolTip = stringBuilder.ToString();

            Pushpin pushpin = new Pushpin();
            pushpin.Uid = breaker1.GlobalId.ToString();
            pushpin.Location = pinLocation;
            pushpin.ToolTip = toolTip;
            pushpin.Cursor = Cursors.Hand;

            if (breaker.Data.Energized == Enums.Energized.FromEnergySRC)
                pushpin.Template = (ControlTemplate)Application.Current.Resources["GreenBreakerTemplate"];
            else if (breaker.Data.Energized == Enums.Energized.FromIsland)
                pushpin.Template = (ControlTemplate)Application.Current.Resources["BlueBreakerTemplate"];
            else
                pushpin.Template = (ControlTemplate)Application.Current.Resources["RedBreakerTemplate"];

            _map.Children.Add(pushpin);
        }
        private void DrowOnMapEnergyConsumer(TreeNode<NodeData> energyConsumer, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            EnergyConsumer eConsumer = (EnergyConsumer)energyConsumer.Data.IdentifiedObject;

            Location pinLocation = new Location(eConsumer.Longitude, eConsumer.Latitude);
            stringBuilder.Append(stringBuilderUniversal);
            stringBuilder.AppendFormat("Name: {0}{1}", eConsumer.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}", eConsumer.Description);
            string toolTip = stringBuilder.ToString();

            Pushpin pushpin = new Pushpin();
            pushpin.Uid = eConsumer.GlobalId.ToString();
            pushpin.Location = pinLocation;
            pushpin.ToolTip = toolTip;
            pushpin.Cursor = Cursors.Hand;
            pushpin.Template = (ControlTemplate)Application.Current.Resources["EnergyConsumerTemplate"];

            _map.Children.Add(pushpin);
        }
        private void DrowOnMapDER(TreeNode<NodeData> der, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Generator derr = (Generator)der.Data.IdentifiedObject;

            Location pinLocation = new Location(derr.Longitude, derr.Latitude);
            stringBuilder.Append(stringBuilderUniversal);
            stringBuilder.AppendFormat("Name: {0}{1}", derr.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}", derr.Description);
            string toolTip = stringBuilder.ToString();

            Pushpin pushpin = new Pushpin();
            pushpin.Uid = derr.GlobalId.ToString();
            pushpin.Location = pinLocation;
            pushpin.ToolTip = toolTip;
            pushpin.Cursor = Cursors.Hand;

            if (derr.GeneratorType == FTN.Common.GeneratorType.Wind)
                pushpin.Template = (ControlTemplate)Application.Current.Resources["WindTurbineTemplate"];
            else
                pushpin.Template = (ControlTemplate)Application.Current.Resources["SolarPanelTemplate"];

            _map.Children.Add(pushpin);
        }
        private void DrowOnMapDiscrete(TreeNode<NodeData> discrete, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Discrete disc = (Discrete)discrete.Data.IdentifiedObject;

            Pushpin pushpin = (Pushpin)_map.Children.OfType<UIElement>().ToList().Where(x => x.Uid == disc.PowerSystemResource.ToString()).FirstOrDefault();

            if (pushpin == null)
                return;

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("----------------------------------------{0}", Environment.NewLine);
            stringBuilder.AppendFormat("Min Value: {0}{1}", disc.MinValue, Environment.NewLine);
            stringBuilder.AppendFormat("Max Value: {0}{1}", disc.MaxValue, Environment.NewLine);

            if (disc.NormalValue == 1)
            {
                stringBuilder.AppendFormat("Current State: OPEN (1)", Environment.NewLine);
            }
            else
            {
                stringBuilder.AppendFormat("Current State: CLOSED (0)", Environment.NewLine);
            }

            //stringBuilder.AppendFormat("Current Value: {0}", disc.NormalValue);

            pushpin.ToolTip += stringBuilder.ToString();
        }
        private void DrowOnMapAnalog(TreeNode<NodeData> analog, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Analog analog1 = (Analog)analog.Data.IdentifiedObject;

            Pushpin pushpin = (Pushpin)_map.Children.OfType<UIElement>().ToList().Where(x => x.Uid == analog1.PowerSystemResource.ToString()).FirstOrDefault();

            if (pushpin == null)
                return;

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("----------------------------------------{0}", Environment.NewLine);
            stringBuilder.AppendFormat("Min Value: {0} kW{1}", analog1.MinValue, Environment.NewLine);
            stringBuilder.AppendFormat("Max Value: {0} kW{1}", analog1.MaxValue, Environment.NewLine);
            stringBuilder.AppendFormat("Current Value: {0} kW", analog1.NormalValue);
            pushpin.ToolTip += stringBuilder.ToString();
        }
        private bool CanDrowAcLine(Enums.Energized energized)
        {
            if (energized == Enums.Energized.NotEnergized && VisibilityOfElements["LineRed"])
                return true;

            if (energized == Enums.Energized.FromIsland && VisibilityOfElements["LineBlue"])
                return true;

            if (energized == Enums.Energized.FromEnergySRC && VisibilityOfElements["LineGreen"])
                return true;

            return false;
        }
        private bool CanDrowBreaker(Enums.Energized energized)
        {
            if (energized == Enums.Energized.NotEnergized && VisibilityOfElements["DERRed"])
                return true;

            if (energized == Enums.Energized.FromIsland && VisibilityOfElements["DERBlue"])
                return true;

            if (energized == Enums.Energized.FromEnergySRC && VisibilityOfElements["DERGreen"])
                return true;

            return false;
        }
        private bool CanDrowGenerator(GeneratorType generatorType)
        {
            if (generatorType == GeneratorType.Solar && VisibilityOfElements["SolarPanel"])
                return true;

            if (generatorType == GeneratorType.Wind && VisibilityOfElements["WindTurbine"])
                return true;

            return false;
        }

        private double getLongitude(TreeNode<NodeData> node)
        {
            if (node.Data.Type == DMSType.ACLINESEGMENT)
            {
                //+
                ACLineSegment acLineSegment = (ACLineSegment)node.Data.IdentifiedObject;
                return acLineSegment.Longitude;
            }
            else if (node.Data.Type == DMSType.BREAKER)
            {
                //+
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                return breaker.Longitude;
            }
            else if (node.Data.Type == DMSType.ENEGRYSOURCE)
            {
                //+
                EnergySource source = (EnergySource)node.Data.IdentifiedObject;
                return source.Longitude;

            }
            else if (node.Data.Type == DMSType.ENERGYCONSUMER)
            {
                //+
                EnergyConsumer consumer = (EnergyConsumer)node.Data.IdentifiedObject;
                return consumer.Longitude;
            }
            else if (node.Data.Type == DMSType.GENERATOR)
            {
                //+
                Generator generator = (Generator)node.Data.IdentifiedObject;
                return generator.Longitude;
            }

            return 0;
        }

        private double getLatitude(TreeNode<NodeData> node)
        {

            if (node.Data.Type == DMSType.ACLINESEGMENT)
            {
                //+
                ACLineSegment acLineSegment = (ACLineSegment)node.Data.IdentifiedObject;
                return acLineSegment.Latitude;
            }
            else if (node.Data.Type == DMSType.BREAKER)
            {
                //+
                Breaker breaker = (Breaker)node.Data.IdentifiedObject;
                return breaker.Latitude;
            }
            else if (node.Data.Type == DMSType.ENEGRYSOURCE)
            {
                //+
                EnergySource source = (EnergySource)node.Data.IdentifiedObject;
                return source.Latitude;

            }
            else if (node.Data.Type == DMSType.ENERGYCONSUMER)
            {
                //+
                EnergyConsumer consumer = (EnergyConsumer)node.Data.IdentifiedObject;
                return consumer.Latitude;
            }
            else if (node.Data.Type == DMSType.GENERATOR)
            {
                //+
                Generator generator = (Generator)node.Data.IdentifiedObject;
                return generator.Latitude;
            }

            return 0;
        }
        #endregion

        #region commands
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand<object>(DoSearch);
                }

                return _searchCommand;
            }
        }

        private void DoSearch(object obj)
        {
            //long gid = (long)Convert.ToDouble(_searchParam);
            //Nadji u stablu entitet koji ti treba na osnovu _search param
            try
            {
                TreeNode<NodeData> node = _tree.Where(x => x.Data.IdentifiedObject.Name == SearchParameter).ToList().First();
                if (node != null && node.Data.Type != DMSType.DISCRETE && node.Data.Type != DMSType.ANALOG)
                {
                    double lon = getLongitude(node);
                    double lat = getLatitude(node);

                    Location pinLocation = new Location(lon, lat);
                    _map.ZoomLevel = 23; // 
                    _map.Center = pinLocation;

                    _gisTextBlock.Text = String.Empty;
                    _gisTextBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0398E2"));
                    _gisTextBlock.AppendText(BuildToolTipOnClick(node));
                }
                else
                {
                    PopUpWindow popUpWindow = new PopUpWindow("Element with selected name does not exist.");
                    popUpWindow.ShowDialog();
                }
            }
            catch
            {
                PopUpWindow popUpWindow = new PopUpWindow("Element with selected name does not exist.");
                popUpWindow.ShowDialog();
            }


        }
        #endregion
    }
}
