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
        private long selectedElement;
        private string energySourceValue;
        private float productionGenerators;
        private float consumption;
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

        public float Consumption
        {
            get
            {
                return consumption;
            }
            set
            {
                consumption = value;
                OnPropertyChanged("Consumption");
            }
        }
        public string EnergySourceValue
        {
            get
            {
                return energySourceValue;
            }
            set
            {
                energySourceValue = value;
                OnPropertyChanged("EnergySourceValue");
            }
        }
        public float ProductionGenerators
        {
            get
            {
                return productionGenerators;
            }
            set
            {
                productionGenerators = value;
                OnPropertyChanged("ProductionGenerators");
            }
        }
        public long SelectedElement
        {
            get
            {
                return selectedElement;
            }
            set
            {
                selectedElement = value;               
                OnPropertyChanged("SelectedElement");
            }
        }
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
            set {
                chartValues1 = value;
                OnPropertyChanged("ChartValues1");
            }
        }

        public IChartValues ChartValues2
        {
            get { return chartValues2; }
            set {
                chartValues2 = value;
                OnPropertyChanged("ChartValues2");
            }
        }

        public IChartValues ChartValues3
        {
            get { return chartValues3; }
            set {
                chartValues3 = value;
                OnPropertyChanged("ChartValues3");
            }
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
        private RelayCommand<long> _selectedEventCommand;
        private MyICommand _optimizationCommand;
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
        public ICommand SelectedEventCommand
        {
            get
            {
                if (_selectedEventCommand == null)
                {
                    _selectedEventCommand = new RelayCommand<long>(SelectedEventCommandExecute);
                }

                return _selectedEventCommand;
            }
        }
        public MyICommand OptimizationCommand
        {
            get
            {
                if (_optimizationCommand == null)
                {
                    _optimizationCommand = new MyICommand(OptimizationCommandExecute);
                }

                return _optimizationCommand;
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
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
			Console.Beep();
            
        }
        public void SelectedEventCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;

            CurrentSelectedGid = gid;
            Console.Beep();
            SetChartValues(gid);
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
			CurrentSelectedGid = gid;
			Console.Beep();
            SetChartValues(gid);
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
            Console.Beep();
            SetChartValues(gid);
        }
        
        public void SubstationCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
            Console.Beep();
            SetChartValues(gid);
        }
        public void SubstationElementCommandExecute(long gid)
        {
            CurrentSelectedGid = gid;
            Console.Beep();
            SetChartValues(gid);
		}
        public void OptimizationCommandExecute()
        {
            var energySourceValue = Optimization();
            EnergySourceValue = energySourceValue.ToString() + " kW";
        }
        public float Optimization() {
            if (GidForOptimization != 0) {
                if (proxy == null)
                    proxy = new CommunicationProxy();

                proxy.Open2();
                energySourceOptimizedValue = 0;
                energySourceOptimizedValue = proxy.sendToCE.UpdateThroughUI(GidForOptimization);
                SetChartValuesAfterOptimization(GidForOptimization);
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

		public float GetIncreaseFlexibility()
		{
			float ret = 0;

			foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModel)
			{
				if(networkModelTreeClasses.GID.Equals(CurrentSelectedGid))
				{
					ret = networkModelTreeClasses.MaxFlexibility;
				}

				foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
				{
					if (geographicalRegionTreeClass.GID.Equals(CurrentSelectedGid))
					{
						ret = geographicalRegionTreeClass.MaxFlexibility;
					}

					foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
					{
						if (geographicalSubRegionTreeClass.GID.Equals(CurrentSelectedGid))
						{
							ret = geographicalSubRegionTreeClass.MaxFlexibility;
						}

						foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
						{
							if (substationTreeClass.GID.Equals(CurrentSelectedGid))
							{
								ret = substationTreeClass.MaxFlexibility;
							}

							foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
							{
								if (substationElementTreeClass.GID.Equals(CurrentSelectedGid))
								{
									ret = substationElementTreeClass.MaxFlexibility;
								}
							}
						}
					}
				}
			}

			return ret;
		}

		public float GetDecreaseFlexibility()
		{
			float ret = 0;

			foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModel)
			{
				if (networkModelTreeClasses.GID.Equals(CurrentSelectedGid))
				{
					ret = networkModelTreeClasses.MinFlexibility;
				}

				foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClasses.GeographicalRegions)
				{
					if (geographicalRegionTreeClass.GID.Equals(CurrentSelectedGid))
					{
						ret = geographicalRegionTreeClass.MinFlexibility;
					}

					foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
					{
						if (geographicalSubRegionTreeClass.GID.Equals(CurrentSelectedGid))
						{
							ret = geographicalSubRegionTreeClass.MinFlexibility;
						}

						foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
						{
							if (substationTreeClass.GID.Equals(CurrentSelectedGid))
							{
								ret = substationTreeClass.MinFlexibility;
							}

							foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
							{
								if (substationElementTreeClass.GID.Equals(CurrentSelectedGid))
								{
									ret = substationElementTreeClass.MinFlexibility;
								}
							}
						}
					}
				}
			}

			return ret;
		}
        
        public void SetChartValues(long gid)
        {
            /*foreach(HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Production.Hourly)
            {
                if(hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    float z = (float)y;
                    ProductionGenerators = z;
                }
            }
            foreach (HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Consumption.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    Consumption = (float)y;

                }
            }
           
            
            ChartValues1 = new ChartValues<double>();
            ChartValues2 = new ChartValues<double>();
            ChartValues3 = new ChartValues<double>();
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = ProductionDerForecastDayAhead[gid].Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction= ProductionDerForecastDayAhead[gid].Production.Hourly.OrderBy(x => x.Time).ToList();
            foreach (HourDataPoint hc in tempList)
            {
                ChartValues2.Add((double)hc.ActivePower);
                ChartValues1.Add((double)hc.ActivePower);

            }
            foreach (HourDataPoint hc in tempListProduction)
            {
                ChartValues3.Add((double)hc.ActivePower);
            }*/
        }

        public void SetChartValuesAfterOptimization(long gid)
        {
			/*
            foreach (HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Consumption.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    ProductionGenerators = (float)y;
                }
            }
            ChartValues2 = new ChartValues<double>();
            ChartValues3 = new ChartValues<double>();
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = ProductionDerForecastDayAhead[gid].Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = ProductionDerForecastDayAhead[gid].Production.Hourly.OrderBy(x => x.Time).ToList();
            foreach (HourDataPoint hc in tempList)
            {
                ChartValues2.Add((double)hc.ActivePower);
                ChartValues3.Add((double)hc.ActivePower);

            }
			*/
        }

    }
}
