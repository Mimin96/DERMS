using DERMSCommon;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
using DermsUI.Resources;
using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UI.Communication;
using UI.Model;
using UI.Resources;
using UI.Resources.MediatorPattern;
using UI.View;

namespace UI.ViewModel
{
    public class DERDashboardUserControlViewModel : BindableBase
    {
        CommunicationProxy proxy;
        DERDashboardUserControl dERDashboardUserControl;
        private SolidColorBrush color1;
        private SolidColorBrush color2;
        private bool showConsumption;
        private bool showDerProduction;
        private bool showGridDemands;
        private static long gidForOptimization;
        private static float energySourceOptimizedValue;
        private Visibility visibilityConsumption;
        private Visibility visibilityDERProduction;
        private Visibility visibilityGridDemands;
        private IChartValues chartValues1;
        private IChartValues chartValues2;
        private IChartValues chartValues3;
		private Dictionary<long, DerForecastDayAhead> ProductionDerForecastDayAhead { get; set; }
		private ClientSideProxy ClientSideProxy { get; set; }
		private CalculationEnginePubSub CalculationEnginePubSub { get; set; }
		#region Properties
		public TreeNode<NodeData> Tree { get; set; }
       
        public SolidColorBrush Color1 { get { return color1; } set { color1 = value; OnPropertyChanged("Color1"); } }
        public SolidColorBrush Color2 { get { return color2; } set { color2 = value; OnPropertyChanged("Color2"); } }
		public long CurrentSelectedGid { get; set; }

		public bool ShowConsumption
        {
            get
            {
                return showConsumption;
            }
            set
            {
                showConsumption = value;
                if (showConsumption == true)
                {
                    VisibilityConsumption = Visibility.Visible;
                }
                else
                {
                    VisibilityConsumption = Visibility.Collapsed;
                }
                OnPropertyChanged("ShowConsumption");
            }
        }

        public bool ShowDerProduction
        {
            get
            {
                return showDerProduction;
            }
            set
            {
                showDerProduction = value;
                if (showDerProduction)
                {
                    VisibilityDERProduction = Visibility.Visible;
                }
                else
                {
                    VisibilityDERProduction = Visibility.Collapsed;
                }
                OnPropertyChanged("ShowDerProduction");
            }
        }

        public long GidForOptimization
        {
            get
            {
                return gidForOptimization;
            }
            set
            {
                gidForOptimization = value;
                OnPropertyChanged("GidForOptimization");
            }
        }

        public bool ShowGridDemands
        {
            get
            {
                return showGridDemands;
            }
            set
            {
                showGridDemands = value;
                if (showGridDemands)
                {
                    VisibilityGridDemands = Visibility.Visible;
                }
                else
                {
                    VisibilityGridDemands = Visibility.Collapsed;
                }
                OnPropertyChanged("ShowGridDemands");
            }
        }

        public IChartValues ChartValues1
        {
            get { return chartValues1; }
            set { chartValues1 = value; }
        }

        public IChartValues ChartValues2
        {
            get { return chartValues2; }
            set { chartValues2 = value; }
        }

        public IChartValues ChartValues3
        {
            get { return chartValues3; }
            set { chartValues3 = value; }
        }

        public Visibility VisibilityConsumption { get { return visibilityConsumption; } set { visibilityConsumption = value; OnPropertyChanged("VisibilityConsumption"); } }
        public Visibility VisibilityDERProduction { get { return visibilityDERProduction; } set { visibilityDERProduction = value; OnPropertyChanged("VisibilityDERProduction"); } }
        public Visibility VisibilityGridDemands { get { return visibilityGridDemands; } set { visibilityGridDemands = value; OnPropertyChanged("VisibilityGridDemands"); } }

        #endregion

        #region TreeView Data adn Commands
        private RelayCommand<long> _networkModelCommand;
        private RelayCommand<long> _geographicalRegionCommand;
        private RelayCommand<long> _geographicalSubRegionCommand;
        private RelayCommand<long> _substationCommand;
        private RelayCommand<long> _substationElementCommand;
        public List<NetworkModelTreeClass> _networkModel;

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

        public ObservableCollection<NetworkModelViewClass> NetworkModelItems { get; set; }

        public DERDashboardUserControlViewModel(DERDashboardUserControl dERDashboardUserControl)
        {

            ShowConsumption = true;
            ShowDerProduction = true;
            ShowGridDemands = true;

            Color1 = new SolidColorBrush(Colors.Green);
            Color2 = new SolidColorBrush(Colors.Purple);
            ChartValues1 = new ChartValues<double>();
            ChartValues1.Add(1.00);
            ChartValues1.Add(3.00);
            ChartValues1.Add(9.00);
            ChartValues1.Add(6.00);
            ChartValues1.Add(9.00);
            ChartValues1.Add(7.00);
            ChartValues1.Add(4.00);

            ChartValues2 = new ChartValues<double>();
            ChartValues2.Add(8.00);
            ChartValues2.Add(4.00);
            ChartValues2.Add(6.00);
            ChartValues2.Add(2.00);
            ChartValues2.Add(3.00);
            ChartValues2.Add(1.00);
            ChartValues2.Add(5.00);

            ChartValues3 = new ChartValues<double>();
            ChartValues3.Add(5.00);
            ChartValues3.Add(8.00);
            ChartValues3.Add(3.00);
            ChartValues3.Add(6.00);
            ChartValues3.Add(2.00);
            ChartValues3.Add(1.00);
            ChartValues3.Add(5.00);

            this.dERDashboardUserControl = dERDashboardUserControl;

            Mediator.Register("DerForecastDayAhead", DERDashboardDerForecastDayAhead);
            Mediator.Register("Flexibility", DERDashboardFlexibility);

			CurrentSelectedGid = 0;

			ClientSideProxy = new ClientSideProxy();
            CalculationEnginePubSub = new CalculationEnginePubSub();
            ClientSideProxy.StartServiceHost(CalculationEnginePubSub);
            ClientSideProxy.Subscribe((int)Enums.Topics.Flexibility);
        }

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
			//GidForOptimization = 0;
			//GidForOptimization = gid;
			CurrentSelectedGid = gid;
			Console.Beep();
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
			CurrentSelectedGid = gid;
			Console.Beep();
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
            Console.Beep();
		}
        public void SubstationCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
            Console.Beep();
		}
        public void SubstationElementCommandExecute(long gid)
        {
            CurrentSelectedGid = gid;
            Console.Beep();
		}

        public float Optimization() {
            if (GidForOptimization != 0) {
                if (proxy == null)
                    proxy = new CommunicationProxy();

                proxy.Open2();
                energySourceOptimizedValue = 0;
                energySourceOptimizedValue = proxy.sendToCE.UpdateThroughUI(GidForOptimization);
            }
            return energySourceOptimizedValue;
        }
        #endregion

        public ChartValues<ObservableValue> Values1 { get; set; }
        public ChartValues<ObservableValue> Values2 { get; set; }

        private void MoveOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();

            foreach (var value in Values1)
            {
                value.Value = r.Next(0, 10);
            }
            foreach (var value in Values2)
            {
                value.Value = r.Next(0, 10);
            }
        }
        public void DERDashboardFlexibility(object parameter)
        {
            // TREBA IMPLEMENTIRATI
            dERDashboardUserControl.ProductionFromGenerators.Value = ((DataToUI)parameter).Flexibility;
        }

        public void DERDashboardDerForecastDayAhead(object parameter)
        {
			ProductionDerForecastDayAhead = ((DataToUI)parameter).Data;
		}

		public Double GetIncreaseFlexibility()
		{
			// TREBA IZ MODELA ILI IZVUCI MAX FLEXIBILITY
			return 139.5;
		}

		public Double GetDecreaseFlexibility()
		{
			// TREBA IZ MODELA ILI IZVUCI MIN FLEXIBILITY
			return 59.5;
		}
	}
}
