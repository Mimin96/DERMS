using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Resources;
using UI.View;
using CalculationEngineServiceCommon;
using DERMSCommon.UIModel;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel.ThreeViewModel;
using CloudCommon.SCADA.AzureStorage.Entities;
using UI.Communication;

namespace UI.ViewModel
{
    public class HistoryUserControlViewModel : BindableBase
    {
        #region Variables
        private bool _min, _max, _avg;
        private Visibility _isVisible;
        private Visibility _isMesec = Visibility.Hidden;
        private Visibility _isGodina = Visibility.Hidden;
        private Visibility _isGodinaa = Visibility.Hidden;
        private Visibility _isDan = Visibility.Hidden;
        private IChartValues _chartValues;
        private IChartValues _chartValuesMonth;
        private IChartValues _chartValuesYear;
        private long _selectedGID;
        private string _selectedPeriod;
        private string _selectedMesec;
        private string _selectedGodina;
        private string _selectedDan;
        private List<NetworkModelTreeClass> _networkModelUI;
        private ObservableCollection<DayItemUI> _itemsDay;
        private ObservableCollection<MonthItemUI> _itemsMonth;
        private ObservableCollection<YearItemUI> _itemsYear;
        //
        private IChartValues _dayMin;
        private IChartValues _dayMax;
        private IChartValues _dayAvg;

        private IChartValues _monthMin;
        private IChartValues _monthMax;
        private IChartValues _monthAvg;

        private IChartValues _yearMin;
        private IChartValues _yearMax;
        private IChartValues _yearAvg;

        //
        //
        #endregion

        public HistoryUserControlViewModel()
        {
            NetworkModelUI = new List<NetworkModelTreeClass>();
            var mapper = new LiveCharts.Configurations.CartesianMapper<double>().X((values, index) => index).Y((values) => values).Fill((v, i) => i == DateTime.Now.Hour ? Brushes.White : Brushes.White).Stroke((v, i) => i == DateTime.Now.Hour ? Brushes.White : Brushes.White);

            LiveCharts.Charting.For<double>(mapper, LiveCharts.SeriesOrientation.Horizontal);
            Period = new List<string>() { "Day", "Month", "Year" };
            Mesec = new List<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            Godina = new List<string>() { "2020", "2019", "2018", "2017", "2016", "2015" };
            _itemsDay = new ObservableCollection<DayItemUI>();
            _itemsMonth = new ObservableCollection<MonthItemUI>();
            _itemsYear = new ObservableCollection<YearItemUI>();
            SelectedPeriod = Period[0];
            IsDan = Visibility.Visible;
            _chartValues = new ChartValues<double>();
            for (int j = 0; j <= 24; j++)
                _chartValues.Add(0.0);
            _chartValuesMonth = new ChartValues<double>();
            for (int j = 0; j <= 31; j++)
                _chartValuesMonth.Add(0.0);
            _chartValuesYear = new ChartValues<double>();
            for (int j = 0; j <= 12; j++)
                _chartValuesYear.Add(0.0);
            _min = false;
            _max = false;
            _avg = false;
            _selectedGID = 0;

            //
            populateInitialLinesValues();
            _isDanMinVisible = Visibility.Hidden;
            _isDanMaxVisible = Visibility.Hidden;
            _isDanAvgVisible = Visibility.Hidden;
            //

        }

        //
        private void populateInitialLinesValues()
        {
            _dayMax = new ChartValues<double>();
            _dayMin = new ChartValues<double>();
            _dayAvg = new ChartValues<double>();
            _monthMin = new ChartValues<double>();
            _monthMax = new ChartValues<double>();
            _monthAvg = new ChartValues<double>();
            _yearMin = new ChartValues<double>();
            _yearMax = new ChartValues<double>();
            _yearAvg = new ChartValues<double>();

            for (int j = 0; j <= 24; j++)
                _dayMax.Add((double)0);

            for (int j = 0; j <= 24; j++)
                _dayMin.Add((double)0);

            for (int j = 0; j <= 24; j++)
                _dayAvg.Add((double)0);

            for (int j = 0; j <= 31; j++)
                _monthMin.Add((double)0);

            for (int j = 0; j <= 31; j++)
                _monthMax.Add((double)0);

            for (int j = 0; j <= 31; j++)
                _monthAvg.Add((double)0);

            for (int j = 0; j <= 13; j++)
                _yearMin.Add((double)0);

            for (int j = 0; j <= 13; j++)
                _yearMax.Add((double)0);

            for (int j = 0; j <= 13; j++)
                _yearAvg.Add((double)0);
        }

        private void PopulateMonthDays(string month)
        {
            _monthMin = new ChartValues<double>();
            _monthMax = new ChartValues<double>();
            _monthAvg = new ChartValues<double>();

            //{ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            if (month == "Jan" || month == "Mar" || month == "May" || month == "Jul" || month == "Aug" || month == "Oct" || month == "Dec")
            {
                for (int j = 0; j <= 32; j++)
                    _monthMin.Add((double)0);

                for (int j = 0; j <= 32; j++)
                    _monthMax.Add((double)0);

                for (int j = 0; j <= 32; j++)
                    _monthAvg.Add((double)0);

            }
            else if (month == "Feb")
            {
                for (int j = 0; j <= 29; j++)
                    _monthMin.Add((double)0);

                for (int j = 0; j <= 29; j++)
                    _monthMax.Add((double)0);

                for (int j = 0; j <= 29; j++)
                    _monthAvg.Add((double)0);
            }
            else
            {
                for (int j = 0; j <= 31; j++)
                    _monthMin.Add((double)0);

                for (int j = 0; j <= 31; j++)
                    _monthMax.Add((double)0);

                for (int j = 0; j <= 31; j++)
                    _monthAvg.Add((double)0);
            }


        }
        public List<NetworkModelTreeClass> NetworkModelUI
        {
            get
            {
                return _networkModelUI;
            }
            set
            {
                _networkModelUI = value;
                OnPropertyChanged("NetworkModelUI");
            }
        }
        public IChartValues DayMin
        {
            get { return _dayMin; }
            set
            {
                _dayMin = value;
                OnPropertyChanged("DayMin");
            }
        }
        public IChartValues DayMax
        {
            get { return _dayMax; }
            set
            {
                _dayMax = value;
                OnPropertyChanged("DayMax");
            }
        }
        public IChartValues DayAvg
        {
            get { return _dayAvg; }
            set
            {
                _dayAvg = value;
                OnPropertyChanged("DayAvg");
            }
        }
        public IChartValues MonthMin
        {
            get { return _monthMin; }
            set
            {
                _monthMin = value;
                OnPropertyChanged("MonthMin");
            }
        }
        public IChartValues MonthMax
        {
            get { return _monthMax; }
            set
            {
                _monthMax = value;
                OnPropertyChanged("MonthMax");
            }
        }
        public IChartValues MonthAvg
        {
            get { return _monthAvg; }
            set
            {
                _monthAvg = value;
                OnPropertyChanged("MonthAvg");
            }
        }
        public IChartValues YearMin
        {
            get { return _yearMin; }
            set
            {
                _yearMin = value;
                OnPropertyChanged("YearMin");
            }
        }
        public IChartValues YearMax
        {
            get { return _yearMax; }
            set
            {
                _yearMax = value;
                OnPropertyChanged("YearMax");
            }
        }
        public IChartValues YearAvg
        {
            get { return _yearAvg; }
            set
            {
                _yearAvg = value;
                OnPropertyChanged("YearAvg");
            }
        }
        private Visibility _isDanMinVisible;
        public Visibility IsDanMinVisible
        {
            get
            {
                if (_min && IsDan == Visibility.Visible)
                    return _isDanMinVisible = Visibility.Visible;
                else
                    return _isDanMinVisible = Visibility.Collapsed;
               // return _isDanMinVisible;
            }
            set
            {
                _isDanMinVisible = value; OnPropertyChanged("IsDanMinVisible");
            }
        }
        private Visibility _isDanMaxVisible;
        public Visibility IsDanMaxVisible
        {
            get
            {
                //if (_max && IsDan == Visibility.Visible)
                //    return Visibility.Visible;
                //else
                //    return Visibility.Collapsed;
                return _isDanMaxVisible;
            }
            set
            {
                _isDanMaxVisible = value; OnPropertyChanged("IsDanMaxVisible");
            }
        }
        private Visibility _isDanAvgVisible;
        public Visibility IsDanAvgVisible
        {
            get
            {
                //if (_avg && IsDan == Visibility.Visible)
                //    return Visibility.Visible;
                //else
                //    return Visibility.Collapsed;
                return _isDanAvgVisible;
            }
            set
            {
                _isDanAvgVisible = value; OnPropertyChanged("IsDanAvgVisible");
            }
        }
        private Visibility _isMonthMinVisible;
        public Visibility IsMonthMinVisible
        {
            get
            {
                if (_min && IsMesec == Visibility.Visible)
                    return _isMonthMinVisible = Visibility.Visible;
                else
                    return _isMonthMinVisible = Visibility.Collapsed;
                // return _isDanMinVisible;
            }
            set
            {
                _isMonthMinVisible = value; OnPropertyChanged("IsMonthMinVisible");
            }
        }
        private Visibility _isMonthMaxVisible;
        public Visibility IsMonthMaxVisible
        {
            get
            {
                return _isMonthMaxVisible;
            }
            set
            {
                _isMonthMaxVisible = value; OnPropertyChanged("IsMonthMaxVisible");
            }
        }
        private Visibility _isMonthAvgVisible;
        public Visibility IsMonthAvgVisible
        {
            get
            {
                return _isMonthAvgVisible;
            }
            set
            {
                _isMonthAvgVisible = value; OnPropertyChanged("IsMonthAvgVisible");
            }
        }
        private Visibility _isYearMinVisible;
        public Visibility IsYearMinVisible
        {
            get
            {
                return _isYearMinVisible;
            }
            set
            {
                _isYearMinVisible = value; OnPropertyChanged("IsYearMinVisible");
            }
        }
        private Visibility _isYearMaxVisible;
        public Visibility IsYearMaxVisible
        {
            get
            {
                return _isYearMaxVisible;
            }
            set
            {
                _isYearMaxVisible = value; OnPropertyChanged("IsYearMaxVisible");
            }
        }
        private Visibility _isYearAvgVisible;
        public Visibility IsYearAvgVisible
        {
            get
            {
                return _isYearAvgVisible;
            }
            set
            {
                _isYearAvgVisible = value; OnPropertyChanged("IsYearAvgVisible");
            }
        }
        //

        #region Properties
        public string SelectedPeriod { get => _selectedPeriod; set { _selectedPeriod = value; OnPropertyChanged("SelectedPeriod"); } }
        public string SelectedMesec { get => _selectedMesec; set { _selectedMesec = value; OnPropertyChanged("SelectedMesec"); } }
        public string SelectedGodina { get => _selectedGodina; set { _selectedGodina = value; OnPropertyChanged("SelectedGodina"); } }
        public string SelectedDan { get => _selectedDan; set { _selectedDan = value; OnPropertyChanged("SelectedDan"); } }
        public List<string> Period { get; set; }
        public List<string> Mesec { get; set; }
        public List<string> Godina { get; set; }
        public List<string> Dan { get; set; }
        public IChartValues ChartValues
        {
            get { return _chartValues; }
            set { _chartValues = value; }
        }

        public IChartValues ChartValuesMonth
        {
            get { return _chartValuesMonth; }
            set { _chartValuesMonth = value; }
        }

        public IChartValues ChartValuesYear
        {
            get { return _chartValuesYear; }
            set { _chartValuesYear = value; }
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

        public Visibility IsMesec
        {
            get
            {
                return _isMesec;
            }
            set
            {
                _isMesec = value; OnPropertyChanged("IsMesec");
            }
        }

        public Visibility IsGodina
        {
            get
            {
                return _isGodina;
            }
            set
            {
                _isGodina = value; OnPropertyChanged("IsGodina");
            }
        }

        public Visibility IsGodinaa
        {
            get
            {
                return _isGodinaa;
            }
            set
            {
                _isGodinaa = value; OnPropertyChanged("IsGodinaa");
            }
        }

        public Visibility IsDan
        {
            get
            {
                return _isDan;
            }
            set
            {
                _isDan = value; OnPropertyChanged("IsDan");
            }
        }
        public ICommand ApplyFiltersCommand
        {
            get
            {
                if (_applyFilterCommand == null)
                {
                    _applyFilterCommand = new RelayCommand<object>(ApplyFilter);
                }

                return _applyFilterCommand;
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
        private RelayCommand<object> _applyFilterCommand;

        public List<NetworkModelTreeClass> NetworkModel
        {
            get
            {
                return _networkModel;
            }
            set
            {
                _networkModel = value;
                NetworkModelUI.Clear();
                
                                foreach (NetworkModelTreeClass treeClass in NetworkModel)
                                    {
                    NetworkModelTreeClass treeClassUI = new NetworkModelTreeClass(treeClass.Name, treeClass.GID, treeClass.Type, treeClass.MinFlexibility, treeClass.MinFlexibility);
                    
                                        foreach (GeographicalRegionTreeClass geographicalRegion in treeClass.GeographicalRegions)
                                            {
                        GeographicalRegionTreeClass geographicalRegionUI = new GeographicalRegionTreeClass(geographicalRegion.Name, geographicalRegion.GID, geographicalRegion.Type, geographicalRegion.MinFlexibility, geographicalRegion.MaxFlexibility);
                        
                                                foreach (GeographicalSubRegionTreeClass geographicalSub in geographicalRegion.GeographicalSubRegions)
                                                    {
                            GeographicalSubRegionTreeClass geographicalSubUI = new GeographicalSubRegionTreeClass(geographicalSub.Name, geographicalSub.GID, geographicalSub.Type, geographicalSub.MinFlexibility, geographicalSub.MaxFlexibility);
                            
                                                        foreach (SubstationTreeClass substation in geographicalSub.Substations)
                                                            {
                                SubstationTreeClass substationUI = new SubstationTreeClass(substation.Name, substation.GID, substation.Type, substation.MinFlexibility, substation.MaxFlexibility);
                                
                                                                foreach (SubstationElementTreeClass substationElement in substation.SubstationElements)
                                                                    {
                                                                        if (substationElement.Type == FTN.Common.DMSType.GENERATOR)
                                        substationUI.SubstationElements.Add(new SubstationElementTreeClass(substationElement.Name, substationElement.GID, substationElement.Type, substationElement.P, substationElement.MinFlexibility, substationElement.MaxFlexibility));
                                                                    }
                                
                                geographicalSubUI.Substations.Add(substationUI);
                                                            }
                            
                            geographicalRegionUI.GeographicalSubRegions.Add(geographicalSubUI);
                                                    }
                        
                        treeClassUI.GeographicalRegions.Add(geographicalRegionUI);
                                            }
                    
                    NetworkModelUI.Add(treeClassUI);
                                    }
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


        public ObservableCollection<DayItemUI> ItemsDay { get => _itemsDay; set { _itemsDay = value; OnPropertyChanged("ItemsDay"); } }
        public ObservableCollection<MonthItemUI> ItemsMonth { get => _itemsMonth; set { _itemsMonth = value; OnPropertyChanged("ItemsMonth"); } }
        public ObservableCollection<YearItemUI> ItemsYear { get => _itemsYear; set { _itemsYear = value; OnPropertyChanged("ItemsYear"); } }
        #endregion

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        { 
            _selectedGID = 0; 
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            _selectedGID = 0;
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            _selectedGID = 0;
        }
        public void SubstationCommandExecute(long gid)
        {
            _selectedGID = 0;
        }
        public void SubstationElementCommandExecute(long gid)
        {
            _selectedGID = gid;
        }
        #endregion

        #region Public Methods

        public void HistorySelect(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            switch (comboBox.SelectedItem)
            {
                case "Month":
                    IsMesec = Visibility.Visible;
                    IsDan = Visibility.Hidden;
                    SelectedMesec = Mesec[0];
                    SelectedGodina = Godina[0];
                    IsGodina = Visibility.Visible;
                    IsGodinaa = Visibility.Hidden;
                    break;
                case "Day":
                    IsMesec = Visibility.Hidden;
                    IsDan = Visibility.Visible;
                    IsGodina = Visibility.Hidden;
                    IsGodinaa = Visibility.Hidden;
                    break;
                case "Year":
                    IsMesec = Visibility.Hidden;
                    IsDan = Visibility.Hidden;
                    IsGodina = Visibility.Visible;
                    IsGodinaa = Visibility.Visible;
                    SelectedGodina = Godina[0];
                    break;
                default:
                    MessageBox.Show("Unexpected type of CheckBox.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        public void HistorySelectDan(object sender, SelectionChangedEventArgs e)
        {
            SelectedDan = e.Source.ToString();
        }

        public void HistorySelectGodina(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            SelectedGodina = comboBox.SelectedItem.ToString();
            //for (int i = 0; i < Godina.Count; i++)
            //    if (SelectedGodina == Godina[i])
            //    {
            //        SelectedGodina = (++i).ToString();
            //        break;
            //    }
        }

        public void HistorySelectMesec(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            SelectedMesec = comboBox.SelectedItem.ToString();
            for (int i = 0; i < Mesec.Count; i++)
                if (SelectedMesec == Mesec[i])
                {
                    SelectedMesec = (++i).ToString();
                    break;
                }
        }
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

        private void ApplyFilter(object obj)
        {
            //
            IsDanMinVisible = Visibility.Hidden;
            IsDanMaxVisible = Visibility.Hidden;
            IsDanAvgVisible = Visibility.Hidden;

            IsMonthMinVisible = Visibility.Hidden;
            IsMonthMaxVisible = Visibility.Hidden;
            IsMonthAvgVisible = Visibility.Hidden;

            IsYearMinVisible = Visibility.Hidden;
            IsYearMaxVisible = Visibility.Hidden;
            IsYearAvgVisible = Visibility.Hidden;
            //

            bool bool1 = false, bool2 = false, bool3 = false;
            if (_selectedGID != 0)
            {
                if (_min || _max || _avg)
                {
                    if (SelectedDan == null && SelectedMesec == null && SelectedGodina == null)
                    {
                        PopUpWindow popUpWindow = new PopUpWindow("You must select time period.");
                        popUpWindow.ShowDialog();
                        return;
                    }

                    UIClientHistory uIClient = new UIClientHistory("GetHistoryEndpoint");
                    List<CollectItemUI> collectItems = new List<CollectItemUI>();

                    populateInitialLinesValues();
                    PopulateMonthDays(SelectedMesec);

                    int hour = 0;
                    int day = 0;
                    int month = 0;
                    int counter = 0;
                    double sum = 0.0;
                    double min = Double.MaxValue;
                    double max = Double.MinValue;

                    //
                    //My day mins by hour
                    if (IsDan == Visibility.Visible)
                    {
                        collectItems = uIClient.GetCollectItemsDateTime(DateTime.Parse(SelectedDan), _selectedGID);
                        collectItems = collectItems.OrderBy(x => x.Timestamp.Hour).ToList();
                    }
                    else if (IsMesec == Visibility.Visible)
                    {
                        DateTime dateTime = new DateTime(Int32.Parse(SelectedGodina), Int32.Parse(SelectedMesec), 1);
                        _itemsDay = new ObservableCollection<DayItemUI>(uIClient.GetDayItemsDateTime(dateTime, _selectedGID));
                        _itemsDay = new ObservableCollection<DayItemUI>(_itemsDay.OrderBy(x=>x.Timestamp).ToList());
                    }
                    else if (IsGodina == Visibility.Visible)
                    {
                        DateTime dateTime = new DateTime(Int32.Parse(SelectedGodina), 1, 1);
                        _itemsMonth = new ObservableCollection<MonthItemUI>(uIClient.GetMonthItemsDateTime(dateTime, _selectedGID));
                        _itemsMonth = new ObservableCollection<MonthItemUI>(_itemsMonth.OrderBy(x => x.Timestamp).ToList());
                    }

                    if (_min && IsDan == Visibility.Visible)
                    {

                        IsDanMinVisible = Visibility.Visible;
                        if (collectItems.Count > 0)
                        {
                            DateTime currentTime = collectItems[0].Timestamp;
                            foreach (var l in collectItems)
                            {
                                //find min for that hour
                                if (currentTime.Hour == l.Timestamp.Hour)
                                {
                                    if (l.P < min)
                                    {
                                        min = l.P;
                                        hour = l.Timestamp.Hour;
                                    }
                                }
                                else
                                {
                                    DayMin[hour] = min;
                                    hour = l.Timestamp.Hour;
                                    currentTime = l.Timestamp;
                                    min = l.P;
                                }
                                //
                                DayMin[hour] = min;
                            }
                            OnPropertyChanged("DayMin");
                        }
                        else
                        {
                            bool1 = true;
                        }
                    }
                    else if (_min && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        IsMonthMinVisible = Visibility.Visible;
                        if (_itemsDay.Count > 0)
                        {
                            //DateTime currentTime = listMonthItems[0].Timestamp;
                            foreach (var l in _itemsDay)
                            {
                                //find min for that hour
                                //if (currentTime.Day == l.Timestamp.Day)
                                //{
                                //    if (l.PMin < min)
                                //    {
                                //        min = l.PMin;
                                //        day = l.Timestamp.Day;
                                //    }
                                //}
                                //else
                                //{
                                //    MonthMin[day] = min;
                                //    day = l.Timestamp.Day;
                                //    currentTime = l.Timestamp;
                                //    min = l.PMin;
                                //}
                                ////
                                //MonthMin[day] = min;
                                MonthMin[l.Timestamp.Day] = l.PMin;

                            }
                            OnPropertyChanged("MonthMin");
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_min && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        IsYearMinVisible = Visibility.Visible;
                        if (_itemsMonth.Count > 0)
                        {
                            //DateTime currentTime = listMonthItems[0].Timestamp;
                            foreach (var l in _itemsMonth)
                            {
                                //if (currentTime.Month == l.Timestamp.Month)
                                //{
                                //    if (l.PMin < min)
                                //    {
                                //        min = l.PMin;
                                //        month = l.Timestamp.Month;
                                //    }
                                //}
                                //else
                                //{
                                //    YearMin[month] = min;
                                //    month = l.Timestamp.Month;
                                //    currentTime = l.Timestamp;
                                //    min = l.PMin;
                                //}
                                //YearMin[month] = min;
                                YearMin[l.Timestamp.Month] = l.PMin;
                            }
                            OnPropertyChanged("YearMin");
                        }
                        else
                        {
                            bool3 = true;
                        }
                    }

                    if (_max && IsDan == Visibility.Visible)
                    {
                        IsDanMaxVisible = Visibility.Visible;
                        if (collectItems.Count > 0)
                        {
                            DateTime currentTime = collectItems[0].Timestamp;
                            foreach (var l in collectItems)
                            {
                                if (currentTime.Hour == l.Timestamp.Hour)
                                {
                                    if (l.P >= max)
                                    {
                                        max = l.P;
                                        hour = l.Timestamp.Hour;
                                    }
                                }
                                else
                                {
                                    DayMax[hour] = max;
                                    hour = l.Timestamp.Hour;
                                    currentTime = l.Timestamp;
                                    max = l.P;
                                }
                                //
                                DayMax[hour] = max;

                            }
                            OnPropertyChanged("DayMax");
                            //_chartValues[hour] = max;
                        }
                        else
                        {
                            bool1 = true;
                        }

                    }
                    else if (_max && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        IsMonthMaxVisible = Visibility.Visible;
                        if (_itemsDay.Count > 0)
                        {
                            //DateTime currentTime = listMonthItems[0].Timestamp;
                            foreach (var l in _itemsDay)
                            {
                                //if (currentTime.Day == l.Timestamp.Day)
                                //{
                                //    if (l.PMax >= max)
                                //    {
                                //        max = l.PMax;
                                //        day = l.Timestamp.Day;
                                //    }
                                //}
                                //else
                                //{
                                //    MonthMax[day] = max;
                                //    day = l.Timestamp.Hour;
                                //    currentTime = l.Timestamp;
                                //    max = l.PMax;
                                //}
                                ////
                                //MonthMax[day] = max;
                                MonthMax[l.Timestamp.Day] = l.PMax;

                            }
                            OnPropertyChanged("MonthMax");
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_max && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        IsYearMaxVisible = Visibility.Visible;
                        if (_itemsMonth.Count > 0)
                        {
                            //DateTime currentTime = listMonthItems[0].Timestamp;
                            foreach (var l in _itemsMonth)
                            {
                                //if (currentTime.Month == l.Timestamp.Month)
                                //{
                                //    if (l.PMax >= max)
                                //    {
                                //        max = l.PMax;
                                //        month = l.Timestamp.Month;
                                //    }
                                //}
                                //else
                                //{
                                //    YearMax[month] = max;
                                //    month = l.Timestamp.Hour;
                                //    currentTime = l.Timestamp;
                                //    max = l.PMax;
                                //}
                                //

                                YearMax[l.Timestamp.Month] = l.PMax;
                            }
                            OnPropertyChanged("YearMax");
                        }
                        else
                        {
                            bool3 = true;
                        }
                    }

                    double sumForHour = 0;
                    //double sumForDay = 0;
                    if (_avg && IsDan == Visibility.Visible)
                    {
                        IsDanAvgVisible = Visibility.Visible;
                        if (collectItems.Count > 0)
                        {

                            DateTime currentTime = collectItems[0].Timestamp;
                            foreach (var l in collectItems)
                            {
                                if (currentTime.Hour == l.Timestamp.Hour)
                                {
                                    sum += l.P;
                                    counter++;
                                    hour = l.Timestamp.Hour;
                                }
                                else
                                {
                                    sumForHour = sum / counter;
                                    DayAvg[hour] = sumForHour;
                                    hour = l.Timestamp.Hour;
                                    currentTime = l.Timestamp;
                                    counter = 1;
                                    sum = l.P;
                                }
                                //
                                sumForHour = sum / counter;
                                DayAvg[hour] = sumForHour;
                            }
                            OnPropertyChanged("DayAvg");
                        }
                        else
                        {
                            bool1 = true;
                        }
                    }
                    else if (_avg && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        IsMonthAvgVisible = Visibility.Visible;
                        if (_itemsDay.Count > 0)
                        {
                            DateTime currentTime = _itemsDay[0].Timestamp;
                            foreach (var l in _itemsDay)
                            {
                                //if (currentTime.Day == l.Timestamp.Day)
                                //{
                                //    sum += l.PAvg;
                                //    counter++;
                                //    day = l.Timestamp.Day;
                                //}
                                //else
                                //{
                                //    sumForDay = sum / counter;
                                //    MonthAvg[day] = sumForDay;
                                //    day = l.Timestamp.Day;
                                //    currentTime = l.Timestamp;
                                //    counter = 1;
                                //    sum = l.PAvg;
                                //}
                                ////
                                //sumForDay = sum / counter;

                                MonthAvg[l.Timestamp.Day] = l.PAvg ;
                            }
                            OnPropertyChanged("MonthAvg");
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_avg && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        double sumForMonth = 0;
                        IsYearAvgVisible = Visibility.Visible;
                        if (_itemsMonth.Count > 0)
                        {
                            //foreach (var l in listYearItems)
                            //{
                            //    sum += l.PAvg;
                            //    counter++;
                            //}

                            //_chartValuesYear[0] = sum / counter;
                            //DateTime currentTime = listYearItems[0].Timestamp;
                            foreach (var l in _itemsMonth)
                            {
                            //    if (currentTime.Month == l.Timestamp.Month)
                            //    {
                            //        sum += l.PAvg;
                            //        counter++;
                            //        month = l.Timestamp.Month;
                            //    }
                            //    else
                            //    {
                            //        sumForMonth = sum / counter;
                            //        YearAvg[month] = sumForMonth;
                            //        month = l.Timestamp.Month;
                            //        currentTime = l.Timestamp;
                            //        counter = 1;
                            //        sum = l.PAvg;
                            //    }
                            //    //
                            //    sumForMonth = sum / counter;
                                YearAvg[l.Timestamp.Month] = l.PAvg;
                            }

                            OnPropertyChanged("YearAvg");

                        }
                        else
                        {
                            bool3 = true;
                        }
                    }

                    if (bool1) 
                    {
                        PopUpWindow popUpWindow = new PopUpWindow("Data for that day doesnt exist.");
                        popUpWindow.ShowDialog();
                    }

                    if (bool2) 
                    {
                        PopUpWindow popUpWindow = new PopUpWindow("Data for that month doesnt exist.");
                        popUpWindow.ShowDialog();
                    }

                    if (bool3)
                    {
                        PopUpWindow popUpWindow = new PopUpWindow("Data for that year doesnt exist.");
                        popUpWindow.ShowDialog();
                    }
                }
                else
                {
                    PopUpWindow popUpWindow = new PopUpWindow("You must select min/max/average box.");
                    popUpWindow.ShowDialog();
                }
            }
            else
            {
                PopUpWindow popUpWindow = new PopUpWindow("Element in tree not selected.");
                popUpWindow.ShowDialog();
            }
        }
        #endregion
    }
}
