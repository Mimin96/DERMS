using DERMSCommon.UIModel;
using DERMSCommon.UIModel.ThreeViewModel;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Resources;

namespace UI.ViewModel
{
    public class HistoryUserControlViewModel : BindableBase
    {
        #region Variables
        private bool _min, _max, _avg;
        private Visibility _isVisible;
        private IChartValues _chartValues;
        private long _selectedGID;
        #endregion

        public HistoryUserControlViewModel()
        {
            Period = new List<string>() { "Day", "Month", "Year" };
            SelectedPeriod = Period[0];
            _min = false;
            _max = false;
            _avg = false;
            _selectedGID = 0;
        }

        #region Properties
        public string SelectedPeriod { get; set; }
        public List<string> Period { get; set; }
        public IChartValues ChartValues
        {
            get { return _chartValues; }
            set { _chartValues = value; }
        }

        public Visibility IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value; OnPropertyChanged("IsVisible");
            }
        }
        #endregion

        #region TreeView Data adn Commands
        private RelayCommand<long> _networkModelCommand;
        private RelayCommand<long> _geographicalRegionCommand;
        private RelayCommand<long> _geographicalSubRegionCommand;
        private RelayCommand<long> _substationCommand;
        private RelayCommand<long> _substationElementCommand;
        private List<NetworkModelTreeClass> _networkModel;

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
        #endregion

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        public void SubstationCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        public void SubstationElementCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        #endregion

        #region Public Methods
        public void HistoryFilter(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            switch (checkBox.Name)
            {
                case "Min":
                    _min = (bool)checkBox.IsChecked;
                    break;
                case "Max":
                    _max = (bool)checkBox.IsChecked;
                    break;
                case "Avg":
                    _avg = (bool)checkBox.IsChecked;
                    break;
                default:
                    MessageBox.Show("Unexpected type of CheckBox.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            DoFiltering();
        }
        #endregion

        #region Private Methods
        private void DoFiltering()
        {
        }
        #endregion
    }
}
