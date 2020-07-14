using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using UI.Model;
using UI.Resources;
using MaterialDesignThemes.Wpf;
using DERMSCommon.UIModel;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using UI.Resources.MediatorPattern;
using System.Threading;

namespace UI.ViewModel
{
    public class NetworkModelUserControlViewModel : BindableBase
    {
        #region TreeView Data adn Commands
        private RelayCommand<long> _networkModelCommand;
        private RelayCommand<long> _geographicalRegionCommand;
        private RelayCommand<long> _geographicalSubRegionCommand;
        private RelayCommand<long> _substationCommand;
        private RelayCommand<long> _substationElementCommand;
        private RelayCommand<object> _filterCommand;
        public List<NetworkModelTreeClass> _networkModel;
        private bool _onlyEnergySrc = true; 

        public List<NetworkModelTreeClass> NetworkModel
        {
            get
            {
                return _networkModel;
            }
            set
            {
                _networkModel = value;
                OnPropertyChanged("NetworkModel");
            }
        }
        public ICommand NetworkModelCommand
        {
            get
            {
                if (_networkModelCommand == null)
                {
                    _networkModelCommand = new RelayCommand<long>(NetworkModelCommandExecute);
                }

                return _networkModelCommand;
            }
        }
        public ICommand GeographicalRegionCommand
        {
            get
            {
                if (_geographicalRegionCommand == null)
                {
                    _geographicalRegionCommand = new RelayCommand<long>(GeographicalRegionCommandExecute);
                }

                return _geographicalRegionCommand;
            }
        }
        public ICommand GeographicalSubRegionCommand
        {
            get
            {
                if (_geographicalSubRegionCommand == null)
                {
                    _geographicalSubRegionCommand = new RelayCommand<long>(GeographicalSubRegionCommandExecute);
                }

                return _geographicalSubRegionCommand;
            }
        }
        public ICommand SubstationCommand
        {
            get
            {
                if (_substationCommand == null)
                {
                    _substationCommand = new RelayCommand<long>(SubstationCommandExecute);
                }

                return _substationCommand;
            }
        }
        public ICommand SubstationElementCommand
        {
            get
            {
                if (_substationElementCommand == null)
                {
                    _substationElementCommand = new RelayCommand<long>(SubstationElementCommandExecute);
                }

                return _substationElementCommand;
            }
        }
        public ICommand FilterCommand
        {
            get
            {
                if (_filterCommand == null)
                {
                    _filterCommand = new RelayCommand<object>(FilterCommandExecute);
                }

                return _filterCommand;
            }
        }
        #endregion

        public NetworkModelUserControlViewModel()
        {
            Mediator.Register("NMSNetworkModelDataNetworkModel", NMSNetworkModelDataNetworkModel);

            PopulateNetworkModelItemsForWholeTree();
            ComboboxItems = new List<ComboData>();
            SelectedItem = new ComboData();
            SearchCriteria = new List<string>();
            SearchCriteria.Add("Name");
            SearchCriteria.Add("Gid");
            SelectedCriteria = SearchCriteria[0];
        }

        #region Properties
        public List<ComboData> ComboboxItems { get; set; }
        public ComboData SelectedItem { get; set; }
        public List<string> SearchCriteria { get; set; }
        public string SelectedCriteria { get; set; }
        public string SearchParameter { get; set; }
        public TreeNode<NodeData> Tree { get; set; }
        public ObservableCollection<NetworkModelViewClass> NetworkModelItems { get; set; }
        #endregion

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
            PopulateNetworkModelItemsForWholeTree();
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();

            TreeNode<NodeData> root = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid).First();

            _onlyEnergySrc = false;
            foreach (TreeNode<NodeData> node in root.Children)
            {
                DealWithChildren(node);
            }
            OnPropertyChanged("NetworkModelItems");
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();

            TreeNode<NodeData> root = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid).First();
            _onlyEnergySrc = false;
            foreach (TreeNode<NodeData> node in root.Children)
            {
                DealWithChildren(node);
            }
            OnPropertyChanged("NetworkModelItems");
        }
        public void SubstationCommandExecute(long gid)
        {
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();

            TreeNode<NodeData> root = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid).First();

            _onlyEnergySrc = false;
            foreach (TreeNode<NodeData> node in root.Children)
            {
                DealWithChildren(node);
            }
            OnPropertyChanged("NetworkModelItems");
        }
        public void SubstationElementCommandExecute(long gid)
        {
            _onlyEnergySrc = true;
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();
            TreeNode<NodeData> node = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid).First();
            DealWithChildren(node);
            OnPropertyChanged("NetworkModelItems");
        }
        private void FilterCommandExecute(object paremeter)
        {
            TreeNode<NodeData> element = null;
            if (SelectedCriteria == "Name")
            {
                try
                {
                    element = Tree.Where(x => x.Data.IdentifiedObject.Name == SearchParameter).First();
                }
                catch
                {

                }

                if (element == null)
                {
                    NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();
                    OnPropertyChanged("NetworkModelItems");
                    return;
                }

            }
            else if (SelectedCriteria == "Gid")
            {
                try
                {
                    element = Tree.Where(x => x.Data.IdentifiedObject.GlobalId.ToString() == SearchParameter).First();

                }
                catch
                {

                }
                if (element == null)
                {
                    NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();
                    OnPropertyChanged("NetworkModelItems");
                    return;
                }
            }

            if (element.Data.Type == FTN.Common.DMSType.ACLINESEGMENT || element.Data.Type == FTN.Common.DMSType.ANALOG || element.Data.Type == FTN.Common.DMSType.BREAKER
                || element.Data.Type == FTN.Common.DMSType.CONNECTIVITYNODE || element.Data.Type == FTN.Common.DMSType.DISCRETE || element.Data.Type == FTN.Common.DMSType.TERMINAL
                )
                return;

            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();
            if (element.Data.Type == FTN.Common.DMSType.ENEGRYSOURCE || element.Data.Type == FTN.Common.DMSType.ENERGYCONSUMER || element.Data.Type == FTN.Common.DMSType.GENERATOR)
            {
                if (element.Data.Type == FTN.Common.DMSType.ENEGRYSOURCE)
                {
                    string infoString = dataString(element);
                    NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightGreen, PackIconKind.TransmissionTower, "Energy source " + element.Data.IdentifiedObject.Name, infoString));
                }
                else
                {
                    DealWithChildren(element);
                }
            }
            else
            {
                foreach (TreeNode<NodeData> node in element.Children)
                {
                    DealWithChildren(node);
                }
            }
            OnPropertyChanged("NetworkModelItems");


        }
        #endregion

        #region Private methods

        private void PopulateNetworkModelItemsForWholeTree()
        {
            if (Tree == null)
                return;
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>();

            TreeNode<NodeData> root = Tree.Where(x => x.IsRoot == true).First();

            _onlyEnergySrc = false;
            foreach (TreeNode<NodeData> node in root.Children)
            {
                DealWithChildren(node);
            }
            OnPropertyChanged("NetworkModelItems");
        }

        private void DealWithChildren(TreeNode<NodeData> node)
        {
            if (node.Data.Type != FTN.Common.DMSType.ENEGRYSOURCE && node.Data.Type != FTN.Common.DMSType.ENERGYCONSUMER && node.Data.Type != FTN.Common.DMSType.GENERATOR)
            {
                foreach (TreeNode<NodeData> child in node.Children)
                {
                    if (NetworkModelItems.Where(x=>x.Info.Contains(child.Data.IdentifiedObject.GlobalId.ToString())).FirstOrDefault() == null)
                        DealWithChildren(child);
                }
                return;
            }

            if (node.Data.Type == FTN.Common.DMSType.ENEGRYSOURCE)
            {
                string infoString = dataString(node);
                NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightGreen, PackIconKind.TransmissionTower, "Energy source " + node.Data.IdentifiedObject.Name, infoString));
                if (_onlyEnergySrc)
                {
                    return;
                }
            }
            else if (node.Data.Type == FTN.Common.DMSType.ENERGYCONSUMER)
            {
                string infoString = dataString(node);
                NetworkModelItems.Add(new NetworkModelViewClass(Brushes.Gold, PackIconKind.HomeCircle, "Consumer " + node.Data.IdentifiedObject.Name, infoString));
            }
            else if (node.Data.Type == FTN.Common.DMSType.GENERATOR)
            {
                string infoString = dataString(node);
                Generator generator = (Generator)node.Data.IdentifiedObject;
                if (generator.GeneratorType == FTN.Common.GeneratorType.Battery)
                {
                    NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightSkyBlue, PackIconKind.Battery, "Generator " + node.Data.IdentifiedObject.Name, infoString));
                }
                else if (generator.GeneratorType == FTN.Common.GeneratorType.Solar)
                {
                    NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightSkyBlue, PackIconKind.SolarPower, "Generator " + node.Data.IdentifiedObject.Name, infoString));
                }
                else if (generator.GeneratorType == FTN.Common.GeneratorType.Wind)
                {
                    NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightSkyBlue, PackIconKind.WindTurbine, "Generator " + node.Data.IdentifiedObject.Name, infoString));
                }
                else
                {
                    NetworkModelItems.Add(new NetworkModelViewClass(Brushes.LightSkyBlue, PackIconKind.PowerPlug, "Generator " + node.Data.IdentifiedObject.Name, infoString));
                }
            }

            foreach (TreeNode<NodeData> child in node.Children)
                DealWithChildren(child);
        }

        private string dataString(TreeNode<NodeData> node)
        {
            string data = string.Empty;
            if (node.Data.Type == FTN.Common.DMSType.ENEGRYSOURCE)
            {
                EnergySource energySource = (EnergySource)node.Data.IdentifiedObject;
                data += "GID: " + energySource.GlobalId + "\n";
                data += "Name:" + energySource.Name + "\n";
                data += "Position: " + energySource.Latitude + ", " + energySource.Longitude + "\n";
                data += "Active power: " + energySource.ActivePower + "\n";

                foreach (long gid in energySource.Measurements)
                {
                    TreeNode<NodeData> measurement = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid && x.Data.Type == FTN.Common.DMSType.ANALOG).First();
                    if (measurement != null)
                    {
                        Analog analogMeas = (Analog)measurement.Data.IdentifiedObject;
                        data += "Measurement value: " + analogMeas.NormalValue + "\n";
                    }
                }


            }
            else if (node.Data.Type == FTN.Common.DMSType.ENERGYCONSUMER)
            {
                EnergyConsumer energyConsumer = (EnergyConsumer)node.Data.IdentifiedObject;
                data += "GID: " + energyConsumer.GlobalId + "\n";
                data += "Position: " + energyConsumer.Latitude + ", " + energyConsumer.Longitude + "\n";
                data += "Name: " + energyConsumer.Name + "\n";
                data += "PFixed: " + energyConsumer.PFixed + "\n";
                data += "QFixed: " + energyConsumer.QFixed + "\n";

                foreach (long gid in energyConsumer.Measurements)
                {
                    TreeNode<NodeData> measurement = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid && x.Data.Type == FTN.Common.DMSType.ANALOG).First();
                    if (measurement != null)
                    {
                        Analog analogMeas = (Analog)measurement.Data.IdentifiedObject;
                        data += "Measurement value: " + analogMeas.NormalValue + "\n";
                    }
                }
            }
            else if (node.Data.Type == FTN.Common.DMSType.GENERATOR)
            {
                Generator generator = (Generator)node.Data.IdentifiedObject;
                data += "GID: " + generator.GlobalId + "\n";
                data += "Position: " + generator.Latitude + ", " + generator.Longitude + "\n";
                data += "Name: " + generator.Name + "\n";
                data += "Type: " + generator.GeneratorType.ToString() + "\n";

                foreach (long gid in generator.Measurements)
                {
                    TreeNode<NodeData> measurement = Tree.Where(x => x.Data.IdentifiedObject.GlobalId == gid && x.Data.Type == FTN.Common.DMSType.ANALOG).First();
                    if (measurement != null)
                    {
                        Analog analogMeas = (Analog)measurement.Data.IdentifiedObject;
                        data += "Measurement " + analogMeas.Mrid + "= " + analogMeas.NormalValue + " kW\n";
                    }
                }
            }
            return data;
        }
        #endregion

        public void NMSNetworkModelDataNetworkModel(object parameter) 
        {
            List<object> obj = (List<object>)parameter;
            Tree = (TreeNode<NodeData>)obj[0];
            NetworkModel = (List<NetworkModelTreeClass>)obj[1];
        }
    }
}
