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

namespace UI.ViewModel
{
    public class GISUserControlViewModel : BindableBase
    {
        #region Variables
        private TreeNode<NodeData> _tree;
        private TextBox _gisTextBlock;
        private Map _map;
        #endregion

        public GISUserControlViewModel(Map map, TextBox gisTextBlock)
        {
            _map = map;
            _gisTextBlock = gisTextBlock;
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
        #endregion

        #region Public Methods
        public void GetCoordinatesOnMouseClick(object sender, MouseButtonEventArgs e)
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

            //otvoriti novi prozor za komandovanje, proslediti selected u DataContext
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
        #endregion

        #region Private Methods
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
                    stringBuilder.AppendFormat("Normal open state: {0}{1}", breaker.NormalOpen.ToString(), Environment.NewLine);
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
                    stringBuilder.AppendFormat("Min Q: {0}     ", generator.MinQ);
                    stringBuilder.AppendFormat("Max Q: {0}{1}", generator.MaxQ, Environment.NewLine);
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
                    stringBuilderFinal.AppendFormat("Normal Value: {0}{1}", discrete.NormalValue, Environment.NewLine);
                }
            }

            return stringBuilderFinal.ToString();
        }
        private void SetTreeOnMap()
        {
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
                        DrowOnMapAcLineSegment(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.BREAKER:
                        DrowOnMapBreaker(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.ENERGYCONSUMER:
                        DrowOnMapEnergyConsumer(node, stringBuilderUniversal);
                        break;
                    case FTN.Common.DMSType.GENERATOR:
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
            polygon.ToolTip = toolTip;

            if (acLine.Data.Energized == Enums.Energized.FromEnergySRC)
                polygon.Stroke = new SolidColorBrush(Colors.Green);
            else if (acLine.Data.Energized == Enums.Energized.FromIsland)
                polygon.Stroke = new SolidColorBrush(Colors.Blue);
            else
                polygon.Stroke = new SolidColorBrush(Colors.Red);

            polygon.Uid = acLineSegment.GlobalId.ToString();
            polygon.StrokeThickness = 2;
            polygon.Opacity = 0.9;
            polygon.Cursor = Cursors.Hand;
            polygon.Locations = new LocationCollection() {
                new Location(pinLocation.Latitude, pinLocation.Longitude),
                new Location(pinLocation.Latitude - 0.003, pinLocation.Longitude + 0.004)
            };

            _map.Children.Add(polygon);
        }
        private void DrowOnMapBreaker(TreeNode<NodeData> breaker, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Breaker breaker1 = (Breaker)breaker.Data.IdentifiedObject;

            Location pinLocation = new Location(breaker1.Longitude, breaker1.Latitude);
            stringBuilder.Append(stringBuilderUniversal);
            stringBuilder.AppendFormat("Name: {0}{1}", breaker1.Name, Environment.NewLine);
            stringBuilder.AppendFormat("Description: {0}", breaker1.Description);
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

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("----------------------------------------{0}", Environment.NewLine);
            stringBuilder.AppendFormat("Min Value: {0}{1}", disc.MinValue, Environment.NewLine);
            stringBuilder.AppendFormat("Max Value: {0}{1}", disc.MaxValue, Environment.NewLine);
            stringBuilder.AppendFormat("Current Value: {0}", disc.NormalValue);
            pushpin.ToolTip += stringBuilder.ToString();
        }
        private void DrowOnMapAnalog(TreeNode<NodeData> analog, string stringBuilderUniversal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Analog analog1 = (Analog)analog.Data.IdentifiedObject;

            Pushpin pushpin = (Pushpin)_map.Children.OfType<UIElement>().ToList().Where(x => x.Uid == analog1.PowerSystemResource.ToString()).FirstOrDefault();

            stringBuilder.Append(Environment.NewLine);
            stringBuilder.AppendFormat("----------------------------------------{0}", Environment.NewLine);
            stringBuilder.AppendFormat("Min Value: {0} kW{1}", analog1.MinValue, Environment.NewLine);
            stringBuilder.AppendFormat("Max Value: {0} kW{1}", analog1.MaxValue, Environment.NewLine);
            stringBuilder.AppendFormat("Current Value: {0} kW", analog1.NormalValue);
            pushpin.ToolTip += stringBuilder.ToString();
        }
        #endregion
    }
}
