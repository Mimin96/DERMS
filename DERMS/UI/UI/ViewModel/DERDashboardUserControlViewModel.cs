using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.UIModel;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class DERDashboardUserControlViewModel : BindableBase, IDisposable
    {
        string currentTime;
        private Thread timerWorker;
        private bool timerThreadStopSignal = true;
        private bool disposed = false;
        CommunicationProxy proxy;
        DERDashboardUserControl dERDashboardUserControl;
        private SolidColorBrush color1;
        private SolidColorBrush color2;
        private bool showConsumption;
        private bool showDerProduction;
        private bool showGridDemands;
        private long selectedElement;
        private bool canOptimizate;
        private string energySourceValue;
        private string productionGenerators;
        private string consumption;
        private static long gidForOptimization;
        private static float energySourceOptimizedValue;
        private Visibility visibilityConsumption;
        private Visibility visibilityDERProduction;
        private Visibility visibilityGridDemands;
        private IChartValues chartValues1;
        private IChartValues chartValues2;
        private IChartValues chartValues3;
        private List<long> OptimizatedElements;
        //private CalculationEnginePubSub CalculationEnginePubSub { get; set; }
        #region Properties
        private Dictionary<long, DerForecastDayAhead> ProductionDerForecastDayAhead { get; set; }
        private List<long> DisableAutomaticOptimization { get; set; }
        private List<Generator> TurnedOffGenerators { get; set; }
        private ClientSideProxy ClientSideProxy { get; set; }
        public ObservableCollection<NetworkModelViewClass> NetworkModelItems { get; set; }
        public string CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
                OnPropertyChanged("CurrentTime");
            }
        }
        public TreeNode<NodeData> Tree { get; set; }

        public SolidColorBrush Color1 { get { return color1; } set { color1 = value; OnPropertyChanged("Color1"); } }
        public SolidColorBrush Color2 { get { return color2; } set { color2 = value; OnPropertyChanged("Color2"); } }
        public long CurrentSelectedGid { get; set; }

        public string Consumption
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
        public string ProductionGenerators
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
            set
            {
                chartValues1 = value;
                OnPropertyChanged("ChartValues1");
            }
        }

        public IChartValues ChartValues2
        {
            get { return chartValues2; }
            set
            {
                chartValues2 = value;
                OnPropertyChanged("ChartValues2");
            }
        }

        public IChartValues ChartValues3
        {
            get { return chartValues3; }
            set
            {
                chartValues3 = value;
                OnPropertyChanged("ChartValues3");
            }
        }

        public ChartValues<ObservableValue> Values1 { get; set; }
        public ChartValues<ObservableValue> Values2 { get; set; }

        public Visibility VisibilityConsumption { get { return visibilityConsumption; } set { visibilityConsumption = value; OnPropertyChanged("VisibilityConsumption"); } }
        public Visibility VisibilityDERProduction { get { return visibilityDERProduction; } set { visibilityDERProduction = value; OnPropertyChanged("VisibilityDERProduction"); } }
        public Visibility VisibilityGridDemands { get { return visibilityGridDemands; } set { visibilityGridDemands = value; OnPropertyChanged("VisibilityGridDemands"); } }
        public CartesianMapper<ObservableValue> mapper { get; set; }
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

        public DERDashboardUserControlViewModel(DERDashboardUserControl dERDashboardUserControl)
        {
            //Consumption = "";
            //ProductionGenerators = "";
            //EnergySourceValue = "";

            //ChartValues1 = new ChartValues<double>();
            //ChartValues3 = new ChartValues<double>();
            //ChartValues2 = new ChartValues<double>();

            ShowConsumption = true;
            ShowDerProduction = true;
            ShowGridDemands = true;

            Color1 = new SolidColorBrush(Colors.Green);
            Color2 = new SolidColorBrush(Colors.Purple);
            OptimizatedElements = new List<long>();
            DisableAutomaticOptimization = new List<long>();
            TurnedOffGenerators = new List<Generator>();

            this.dERDashboardUserControl = dERDashboardUserControl;

           // Mediator.Register("DerForecastDayAhead", DERDashboardDerForecastDayAhead);
            //Mediator.Register("Flexibility", DERDashboardFlexibility);

            CurrentSelectedGid = 0;

           // ClientSideProxy =  ClientSideProxy.Instance;
            //CalculationEnginePubSub = new CalculationEnginePubSub();
            //ClientSideProxy.StartServiceHost(CalculationEnginePubSub);
            //ClientSideProxy.Subscribe((int)Enums.Topics.Flexibility);
            dERDashboardUserControl.ManualCommanding.IsEnabled = false;

            timerWorker = new Thread(TimerWorker_DoWork);
            timerWorker.Name = "Timer Thread";
            timerWorker.Start();


        }

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;
            GetAllGeoRegions();
            dERDashboardUserControl.ManualCommanding.IsEnabled = true;
            float x;
            if (OptimizatedElements.Contains(gid))
            {
                x = CalculateDemandForSource();
                string temp = String.Format("{0:0.00}", x);
                EnergySourceValue = temp;
            }
            else
            {
                //EnergySourceValue = "0";
            }

        }
        public void SelectedEventCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;

            CurrentSelectedGid = gid;

        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;

            SetChartValues(gid);
            dERDashboardUserControl.ManualCommanding.IsEnabled = true;
            float x;
            if (OptimizatedElements.Contains(gid))
            {
                x = CalculateDemand(gid);
                string temp = String.Format("{0:0.00}", x);
                EnergySourceValue = temp;
            }
            else
            {
                // EnergySourceValue = "0";
            }
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;

            SetChartValues(gid);
            dERDashboardUserControl.ManualCommanding.IsEnabled = true;
            float x;
            if (OptimizatedElements.Contains(gid))
            {
                x = CalculateDemand(gid);
                string temp = String.Format("{0:0.00}", x);
                EnergySourceValue = temp;
            }
            else
            {
                //EnergySourceValue = "0";
            }
        }

        public void SubstationCommandExecute(long gid)
        {
            GidForOptimization = 0;
            GidForOptimization = gid;
            CurrentSelectedGid = gid;

            SetChartValues(gid);
            dERDashboardUserControl.ManualCommanding.IsEnabled = true;
            float x;
            if (OptimizatedElements.Contains(gid))
            {
                x = CalculateDemand(gid);
                string temp = String.Format("{0:0.00}", x);
                EnergySourceValue = temp;
            }
            else
            {
                //EnergySourceValue = "0";
            }
        }
        public void SubstationElementCommandExecute(long gid)
        {
            CurrentSelectedGid = gid;
            proxy = new CommunicationProxy();
            proxy.Open2();
            TurnedOffGenerators = proxy.sendToCE.GeneratorOffCheck();
            foreach (Generator g in TurnedOffGenerators)
            {
                if (g.GlobalId.Equals(gid))
                {
                    dERDashboardUserControl.ManualCommanding.IsEnabled = false;
                    return;
                }
            }

            foreach (NetworkModelTreeClass networkModelTreeClass in NetworkModel)
            {
                foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in networkModelTreeClass.GeographicalRegions)
                {
                    foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
                    {
                        foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
                        {
                            if (substationTreeClass.SubstationElements.Exists(x => x.GID.Equals(gid)))
                            {
                                SubstationElementTreeClass generator = substationTreeClass.SubstationElements.Where(x => x.GID.Equals(gid)).FirstOrDefault();

                                if (generator.Type.Equals(DMSType.GENERATOR))
                                    dERDashboardUserControl.ManualCommanding.IsEnabled = true;
                                else
                                    dERDashboardUserControl.ManualCommanding.IsEnabled = false;

                                return;
                            }
                        }

                    }
                }
            }
            
        }
        public void OptimizationCommandExecute()
        {
            var energySourceValue = Optimization();

            if (canOptimizate)
            {
                string temp = String.Format("{0:0.00}", energySourceValue);
                EnergySourceValue = temp;
                CheckOptimizedDER(GidForOptimization);

                Event e = new Event("Automatic optimization is executed. ", Enums.Component.CalculationEngine, DateTime.Now);
                EventsLogger el = new EventsLogger();
                el.WriteToFile(e);
            }

        }
        public float Optimization()
        {
            if (GidForOptimization != 0 && GidForOptimization != -1)
            {
                proxy = new CommunicationProxy();
                proxy.Open2();
                TurnedOffGenerators = proxy.sendToCE.ListOffTurnedOffGenerators();
                DisableAutomaticOptimization = proxy.sendToCE.ListOfDisabledGenerators();
                
                if (!DisableAutomaticOptimization.Contains(GidForOptimization))
                {
                    if (proxy == null)
                        proxy = new CommunicationProxy();
                    //proxy.Open2();
                    energySourceOptimizedValue = 0;
                    float x = proxy.sendToCE.UpdateThroughUI(GidForOptimization);
                    energySourceOptimizedValue = CalculateDemand(GidForOptimization);
                    DisableOptimization(GidForOptimization);
                    proxy.sendToCE.AllowOptimization(GidForOptimization);
                    canOptimizate = true;
                }
                else
                {
                    PopUpWindow popUpWindow = new PopUpWindow("This part of the network is already in optimal condition.");
                    popUpWindow.ShowDialog();
                    canOptimizate = false;
                }

            }
            else if (GidForOptimization == -1)
            {
                proxy = new CommunicationProxy();
                proxy.Open2();
                TurnedOffGenerators = proxy.sendToCE.ListOffTurnedOffGenerators();
                DisableAutomaticOptimization = proxy.sendToCE.ListOfDisabledGenerators();

                if (!DisableAutomaticOptimization.Contains(GidForOptimization))
                {
                    if (proxy == null)
                        proxy = new CommunicationProxy();
                    energySourceOptimizedValue = 0;
                    //proxy.Open2();
                    float x = proxy.sendToCE.BalanceNetworkModel();
                    energySourceOptimizedValue = CalculateDemandForSource();
                    DisableOptimization(GidForOptimization);
                    proxy.sendToCE.AllowOptimization(GidForOptimization);
                }
                else
                {
                    PopUpWindow popUpWindow = new PopUpWindow("This part of the network is already in optimal condition.");
                    popUpWindow.ShowDialog();
                }

            }
            return energySourceOptimizedValue;
        }
        #endregion

        private void TimerWorker_DoWork()
        {
            while (timerThreadStopSignal)
            {
                if (disposed)
                    return;

                var mapper = new LiveCharts.Configurations.CartesianMapper<double>().X((values, index) => index).Y((values) => values).Fill((v, i) => i == DateTime.Now.Hour ? Brushes.Green : Brushes.White).Stroke((v, i) => i == DateTime.Now.Hour ? Brushes.Green : Brushes.White);

                LiveCharts.Charting.For<double>(mapper, LiveCharts.SeriesOrientation.Horizontal);

                CurrentTime = DateTime.Now.ToString("HH:mm:ss");
                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            disposed = true;
            timerThreadStopSignal = false;
        }

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
            if (GidForOptimization != -1 && GidForOptimization != 0)
                SetChartValuesAfterOptimization(GidForOptimization);
            else if (GidForOptimization == -1)
                GetAllGeoRegions();
        }

        public float GetIncreaseFlexibility()
        {
            float ret = 0;

            foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModel)
            {
                if (networkModelTreeClasses.GID.Equals(CurrentSelectedGid))
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

            foreach (HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Production.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    float z = (float)y;
                    ProductionGenerators = temp;
                }
            }
            foreach (HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Consumption.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    Consumption = temp;

                }
            }


            ChartValues1 = new ChartValues<double>();
            ChartValues2 = new ChartValues<double>();
            ChartValues3 = new ChartValues<double>();
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = ProductionDerForecastDayAhead[gid].Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = ProductionDerForecastDayAhead[gid].Production.Hourly.OrderBy(x => x.Time).ToList();
            foreach (HourDataPoint hc in tempList)
            {
                ChartValues2.Add((double)hc.ActivePower);
                
                // ChartValues1.Add((double)hc.ActivePower);

            }
            foreach (HourDataPoint hc in tempListProduction)
            {
                ChartValues3.Add((double)hc.ActivePower);
            }
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            ChartValues1.Add((double)(hdpConsumption.ActivePower - hdpProduction.ActivePower));
                        else
                            ChartValues1.Add((double)(hdpConsumption.ActivePower - hdpProduction.ActivePower));
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                double chart1 = (double)ChartValues1[i];
                ChartValues1.Add(chart1);
                double chart2 = (double)ChartValues2[i];
                ChartValues2.Add(chart2);
                double chart3 = (double)ChartValues3[i];
                ChartValues3.Add(chart3);


            }
            ChartValues1.RemoveAt(0);
            ChartValues2.RemoveAt(0);
            ChartValues3.RemoveAt(0);
            ChartValues1.RemoveAt(1);
            ChartValues2.RemoveAt(1);
            ChartValues3.RemoveAt(1);
            float tempSource = (float)0.0;
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time) && hdpConsumption.Time.Hour.Equals(DateTime.Now.Hour))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                        else
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                    }
                }
            }
            string tempx = String.Format("{0:0.00}", tempSource);
            EnergySourceValue = "0";
            EnergySourceValue = tempx;
        }

        public void SetChartValuesAfterOptimization(long gid)
        {

            foreach (HourDataPoint hdp in ProductionDerForecastDayAhead[gid].Production.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    ProductionGenerators = temp;
                }
            }
            ChartValues1 = new ChartValues<double>();
            ChartValues2 = new ChartValues<double>();

            ChartValues3 = new ChartValues<double>();
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = ProductionDerForecastDayAhead[gid].Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = ProductionDerForecastDayAhead[gid].Production.Hourly.OrderBy(x => x.Time).ToList();
            foreach (HourDataPoint hc in tempList)
            {
                ChartValues2.Add((double)hc.ActivePower);
               

            }
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                        {
                            ChartValues1.Add((double)hdpConsumption.ActivePower - hdpProduction.ActivePower);
                            ChartValues3.Add((double)(hdpProduction.ActivePower));
                        }
                        else
                        {
                            ChartValues1.Add((double)(hdpConsumption.ActivePower - hdpProduction.ActivePower));
                            ChartValues3.Add((double)hdpProduction.ActivePower);
                        }
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                double chart1 = (double)ChartValues1[i];
                ChartValues1.Add(chart1);
                double chart2 = (double)ChartValues2[i];
                ChartValues2.Add(chart2);
                double chart3 = (double)ChartValues3[i];
                ChartValues3.Add(chart3);


            }
            ChartValues1.RemoveAt(0);
            ChartValues2.RemoveAt(0);
            ChartValues3.RemoveAt(0);
            ChartValues1.RemoveAt(1);
            ChartValues2.RemoveAt(1);
            ChartValues3.RemoveAt(1);
            float tempSource = (float)0.0;
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time) && hdpConsumption.Time.Hour.Equals(DateTime.Now.Hour))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                        else
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                    }
                }
            }
            string tempx = String.Format("{0:0.00}", tempSource);
            EnergySourceValue = "0";
            EnergySourceValue = tempx;

        }

        public void GetAllGeoRegions()
        {
            if (proxy == null)
                proxy = new CommunicationProxy();
            List<long> geoReg = new List<long>();
            proxy.Open2();
            geoReg = proxy.sendToCE.AllGeoRegions();
            DerForecastDayAhead NetworkProduction = new DerForecastDayAhead();
            foreach (long region in geoReg)
            {

                NetworkProduction.Consumption += ProductionDerForecastDayAhead[region].Consumption;
                NetworkProduction.Production += ProductionDerForecastDayAhead[region].Production;
            }

            foreach (HourDataPoint hdp in NetworkProduction.Production.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    float z = (float)y;
                    ProductionGenerators = temp;
                }
            }
            foreach (HourDataPoint hdp in NetworkProduction.Consumption.Hourly)
            {
                if (hdp.Time.Hour.Equals(DateTime.Now.Hour))
                {
                    float x = hdp.ActivePower;
                    string temp = String.Format("{0:0.00}", x);
                    double y = Double.Parse(temp);
                    Consumption = temp;

                }
            }

            ChartValues1 = new ChartValues<double>();
            ChartValues2 = new ChartValues<double>();
            ChartValues3 = new ChartValues<double>();
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = NetworkProduction.Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = NetworkProduction.Production.Hourly.OrderBy(x => x.Time).ToList();
            foreach (HourDataPoint hc in tempList)
            {
                ChartValues2.Add((double)hc.ActivePower);

            }
            foreach (HourDataPoint hc in tempListProduction)
            {
                ChartValues3.Add((double)hc.ActivePower);
            }
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            ChartValues1.Add((double)(hdpConsumption.ActivePower - hdpProduction.ActivePower));
                        else
                            ChartValues1.Add((double)(hdpConsumption.ActivePower - hdpProduction.ActivePower));
                    }
                }
            }

            for (int i = 0; i < 2; i++)
            {
                double chart1 = (double)ChartValues1[i];
                ChartValues1.Add(chart1);
                double chart2 = (double)ChartValues2[i];
                ChartValues2.Add(chart2);
                double chart3 = (double)ChartValues3[i];
                ChartValues3.Add(chart3);


            }
            ChartValues1.RemoveAt(0);
            ChartValues2.RemoveAt(0);
            ChartValues3.RemoveAt(0);
            ChartValues1.RemoveAt(1);
            ChartValues2.RemoveAt(1);
            ChartValues3.RemoveAt(1);
            float tempSource = (float)0.0;
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time) && hdpConsumption.Time.Hour.Equals(DateTime.Now.Hour))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                        else
                            tempSource = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                    }
                }
            }
            string tempx = String.Format("{0:0.00}", tempSource);
            EnergySourceValue = "0";
            EnergySourceValue = tempx;


        }
        public float CalculateDemand(long gid)
        {
            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = ProductionDerForecastDayAhead[gid].Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = ProductionDerForecastDayAhead[gid].Production.Hourly.OrderBy(x => x.Time).ToList();
            float y = (float)0;
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time) && hdpConsumption.Time.Hour.Equals(DateTime.Now.Hour))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            y = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                        else
                            y = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                    }
                }
            }
            return y;

        }
        public float CalculateDemandForSource()
        {
            if (proxy == null)
                proxy = new CommunicationProxy();
            List<long> geoReg = new List<long>();
            proxy.Open2();
            geoReg = proxy.sendToCE.AllGeoRegions();
            DerForecastDayAhead NetworkProduction = new DerForecastDayAhead();
            foreach (long region in geoReg)
            {

                NetworkProduction.Consumption += ProductionDerForecastDayAhead[region].Consumption;
                NetworkProduction.Production += ProductionDerForecastDayAhead[region].Production;
            }

            List<HourDataPoint> tempList = new List<HourDataPoint>();
            List<HourDataPoint> tempListProduction = new List<HourDataPoint>();
            tempList = NetworkProduction.Consumption.Hourly.OrderBy(x => x.Time).ToList();
            tempListProduction = NetworkProduction.Production.Hourly.OrderBy(x => x.Time).ToList();
            float y = (float)0.0;
            foreach (HourDataPoint hdpProduction in tempListProduction)
            {
                foreach (HourDataPoint hdpConsumption in tempList)
                {

                    if (hdpConsumption.Time.Equals(hdpProduction.Time) && hdpConsumption.Time.Hour.Equals(DateTime.Now.Hour))
                    {
                        if (hdpConsumption.ActivePower >= hdpProduction.ActivePower)
                            y = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                        else
                            y = hdpConsumption.ActivePower - hdpProduction.ActivePower;
                    }
                }
            }
            return y;

        }

        public void CheckOptimizedDER(long gid)
        {
            foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModel)
            {
                if (gid.Equals(networkModelTreeClasses.GID))
                {
                    if (!OptimizatedElements.Contains(networkModelTreeClasses.GID))
                    {
                        OptimizatedElements.Add(networkModelTreeClasses.GID);
                    }
                    foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                    {
                        if (!OptimizatedElements.Contains(gr.GID))
                        {
                            OptimizatedElements.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!OptimizatedElements.Contains(sgr.GID))
                            {
                                OptimizatedElements.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!OptimizatedElements.Contains(sub.GID))
                                {
                                    OptimizatedElements.Add(sub.GID);
                                }

                            }

                        }
                    }
                }


                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {
                    if (gid.Equals(gr.GID))
                    {
                        if (!OptimizatedElements.Contains(gr.GID))
                        {
                            OptimizatedElements.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!OptimizatedElements.Contains(sgr.GID))
                            {
                                OptimizatedElements.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!OptimizatedElements.Contains(sub.GID))
                                {
                                    OptimizatedElements.Add(sub.GID);
                                }

                            }

                        }
                        if (!OptimizatedElements.Contains(networkModelTreeClasses.GID))
                        {
                            OptimizatedElements.Add(networkModelTreeClasses.GID);
                        }
                    }

                }

                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {
                        if (gid.Equals(sgr.GID))
                        {
                            if (!OptimizatedElements.Contains(sgr.GID))
                            {
                                OptimizatedElements.Add(sgr.GID);
                            }

                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!OptimizatedElements.Contains(sub.GID))
                                {
                                    OptimizatedElements.Add(sub.GID);
                                }

                            }
                            if (!OptimizatedElements.Contains(networkModelTreeClasses.GID))
                            {
                                OptimizatedElements.Add(networkModelTreeClasses.GID);
                            }
                            if (!OptimizatedElements.Contains(gr.GID))
                            {
                                OptimizatedElements.Add(gr.GID);
                            }

                        }

                    }


                }
                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {

                        foreach (SubstationTreeClass sub in sgr.Substations)
                        {
                            if (gid.Equals(sub.GID))
                            {
                                if (!OptimizatedElements.Contains(sub.GID))
                                    OptimizatedElements.Add(sub.GID);

                                if (!OptimizatedElements.Contains(networkModelTreeClasses.GID))
                                {
                                    OptimizatedElements.Add(networkModelTreeClasses.GID);
                                }
                                if (!OptimizatedElements.Contains(gr.GID))
                                {
                                    OptimizatedElements.Add(gr.GID);
                                }
                                if (!OptimizatedElements.Contains(sgr.GID))
                                {
                                    OptimizatedElements.Add(sgr.GID);
                                }
                            }

                        }

                    }

                }

            }
        }
        private void DisableOptimization(long gid)
        {
            foreach (NetworkModelTreeClass networkModelTreeClasses in NetworkModel)
            {
                if (gid.Equals(networkModelTreeClasses.GID))
                {
                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                    {
                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                    }
                    foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                    {
                        if (!DisableAutomaticOptimization.Contains(gr.GID))
                        {
                            DisableAutomaticOptimization.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }
                        }
                    }
                }

                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {
                    if (gid.Equals(gr.GID))
                    {
                        if (!DisableAutomaticOptimization.Contains(gr.GID))
                        {
                            DisableAutomaticOptimization.Add(gr.GID);
                        }
                        foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }
                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }

                        }
                        if (networkModelTreeClasses.GeographicalRegions.Count == 1)
                        {
                            if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                            {
                                DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                            }
                        }
                        bool tempProvera = true;
                        foreach (var item in networkModelTreeClasses.GeographicalRegions)
                        {
                            if (!DisableAutomaticOptimization.Contains(item.GID))
                            {
                                tempProvera = false;
                                break;
                            }
                        }
                        if (tempProvera)
                        {
                            if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                            {
                                DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                            }
                        }
                    }

                }

                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {
                        if (gid.Equals(sgr.GID))
                        {
                            if (!DisableAutomaticOptimization.Contains(sgr.GID))
                            {
                                DisableAutomaticOptimization.Add(sgr.GID);
                            }

                            foreach (SubstationTreeClass sub in sgr.Substations)
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                {
                                    DisableAutomaticOptimization.Add(sub.GID);
                                }

                            }

                            if (gr.GeographicalSubRegions.Count == 1)
                            {
                                if (!DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    DisableAutomaticOptimization.Add(gr.GID);
                                }
                            }
                            bool tempProvera = true;
                            foreach (var item in gr.GeographicalSubRegions)
                            {
                                if (!DisableAutomaticOptimization.Contains(item.GID))
                                {
                                    tempProvera = false;
                                    break;
                                }
                            }
                            if (tempProvera)
                            {
                                if (!DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    DisableAutomaticOptimization.Add(gr.GID);
                                }
                            }
                            if (networkModelTreeClasses.GeographicalRegions.Count == 1 && DisableAutomaticOptimization.Contains(gr.GID))
                            {
                                if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                {
                                    DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                }
                            }
                            tempProvera = true;
                            foreach (var item in networkModelTreeClasses.GeographicalRegions)
                            {
                                if (!DisableAutomaticOptimization.Contains(item.GID))
                                {
                                    tempProvera = false;
                                    break;
                                }
                            }
                            if (tempProvera)
                            {
                                if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                {
                                    DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                }
                            }

                        }

                    }
                }
                foreach (GeographicalRegionTreeClass gr in networkModelTreeClasses.GeographicalRegions)
                {

                    foreach (GeographicalSubRegionTreeClass sgr in gr.GeographicalSubRegions)
                    {

                        foreach (SubstationTreeClass sub in sgr.Substations)
                        {
                            if (gid.Equals(sub.GID))
                            {
                                if (!DisableAutomaticOptimization.Contains(sub.GID))
                                    DisableAutomaticOptimization.Add(sub.GID);

                                if (sgr.Substations.Count == 1)
                                {
                                    if (!DisableAutomaticOptimization.Contains(sgr.GID))
                                    {
                                        DisableAutomaticOptimization.Add(sgr.GID);
                                    }
                                }
                                bool tempProvera = true;
                                foreach (var item in sgr.Substations)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(sgr.GID))
                                    {
                                        DisableAutomaticOptimization.Add(sgr.GID);
                                    }
                                }
                                tempProvera = true;
                                if (gr.GeographicalSubRegions.Count == 1 && DisableAutomaticOptimization.Contains(sgr.GID))
                                {
                                    if (!DisableAutomaticOptimization.Contains(gr.GID))
                                    {
                                        DisableAutomaticOptimization.Add(gr.GID);
                                    }
                                }
                                foreach (var item in gr.GeographicalSubRegions)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(gr.GID))
                                    {
                                        DisableAutomaticOptimization.Add(gr.GID);
                                    }
                                }
                                if (networkModelTreeClasses.GeographicalRegions.Count == 1 && DisableAutomaticOptimization.Contains(gr.GID))
                                {
                                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                    {
                                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                    }
                                }
                                tempProvera = true;
                                foreach (var item in networkModelTreeClasses.GeographicalRegions)
                                {
                                    if (!DisableAutomaticOptimization.Contains(item.GID))
                                    {
                                        tempProvera = false;
                                        break;
                                    }
                                }
                                if (tempProvera)
                                {
                                    if (!DisableAutomaticOptimization.Contains(networkModelTreeClasses.GID))
                                    {
                                        DisableAutomaticOptimization.Add(networkModelTreeClasses.GID);
                                    }
                                }


                            }

                        }

                    }



                }

            }
        }
    }
}
