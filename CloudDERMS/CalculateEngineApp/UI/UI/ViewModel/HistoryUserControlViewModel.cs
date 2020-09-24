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
        private ObservableCollection<DayItemUI> _itemsDay;
        private ObservableCollection<MonthItemUI> _itemsMonth;
        private ObservableCollection<YearItemUI> _itemsYear;

        #endregion

        public HistoryUserControlViewModel()
        {
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

        }

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
                    _itemsDay = new ObservableCollection<DayItemUI>(uIClient.GetDayItems());
                    _itemsMonth = new ObservableCollection<MonthItemUI>(uIClient.GetMonthItems());
                    _itemsYear = new ObservableCollection<YearItemUI>(uIClient.GetYearItems());

                    for (int j = 0; j <= 24; j++)
                        _chartValues[j] = -100.0;
                    for (int j = 0; j <= 31; j++)
                        _chartValuesMonth[j] = -100.0;
                    for (int j = 0; j <= 12; j++)
                        _chartValuesYear[j] = -100.0;
                    int hour = 0;
                    int day = 0;
                    int month = 0;
                    int counter = 0;
                    double sum = 0.0;
                    double min = Double.MaxValue;
                    double max = Double.MinValue;

                    List<DayItemUI> listDayItems = new List<DayItemUI>();
                    List<MonthItemUI> listMonthItems = new List<MonthItemUI>();
                    List<YearItemUI> listYearItems = new List<YearItemUI>();

                    foreach (var i in _itemsDay)
                    {
                        if (i.Gid == _selectedGID && SelectedDan == i.Timestamp.Date.ToString())
                            listDayItems.Add(i);
                    }

                    foreach (var i in _itemsYear)
                    {
                        if (i.Gid == _selectedGID && SelectedGodina == i.Timestamp.Year.ToString())
                            listYearItems.Add(i);
                    }

                    foreach (var i in _itemsMonth)
                    {
                        if (i.Gid == _selectedGID && SelectedMesec == i.Timestamp.Month.ToString() && SelectedGodina == i.Timestamp.Year.ToString())
                            listMonthItems.Add(i);
                    }
                    //
                    //

                    if (_min && IsDan == Visibility.Visible)
                    {
                        if (listDayItems.Count > 0)
                        {
                            foreach (var l in listDayItems)
                            {
                                if (l.PMin < min)
                                {
                                    min = l.PMin;
                                    hour = l.Timestamp.Hour;
                                }
                            }
                            _chartValues[hour] = min;
                        }
                        else
                        {
                            bool1 = true;
                        }

                    }
                    else if (_min && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        if (listMonthItems.Count > 0)
                        {
                            foreach (var l in listMonthItems)
                            {
                                if (l.PMin < min)
                                {
                                    min = l.PMin;
                                    day = l.Timestamp.Day;
                                }
                            }
                            _chartValuesMonth[day] = min;
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_min && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        if (listYearItems.Count > 0)
                        {
                            foreach (var l in listYearItems)
                            {
                                if (l.PMin < min)
                                {
                                    min = l.PMin;
                                    month = l.Timestamp.Month;
                                }
                            }
                            _chartValuesYear[month] = min;
                        }
                        else
                        {
                            bool3 = true;
                        }
                    }

                    if (_max && IsDan == Visibility.Visible)
                    {
                        if (listDayItems.Count > 0)
                        {
                            foreach (var l in listDayItems)
                            {
                                if (l.PMax >= max)
                                {
                                    max = l.PMax;
                                    hour = l.Timestamp.Hour;
                                }

                            }
                            _chartValues[hour] = max;
                        }
                        else
                        {
                            bool1 = true;
                        }

                    }
                    else if (_max && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        if (listMonthItems.Count > 0)
                        {
                            foreach (var l in listMonthItems)
                            {
                                if (l.PMax >= max)
                                {
                                    max = l.PMax;
                                    day = l.Timestamp.Day;
                                }
                            }
                            _chartValuesMonth[day] = max;
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_max && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        if (listYearItems.Count > 0)
                        {
                            foreach (var l in listYearItems)
                            {
                                if (l.PMax >= max)
                                {
                                    max = l.PMax;
                                    month = l.Timestamp.Month;
                                }
                            }
                            _chartValuesYear[month] = max;
                        }
                        else
                        {
                            bool3 = true;
                        }
                    }

                    if (_avg && IsDan == Visibility.Visible)
                    {
                        if (listDayItems.Count > 0)
                        {
                            foreach (var l in listDayItems)
                            {
                                sum += l.PAvg;
                                counter++;
                            }

                            _chartValues[0] = sum / counter;
                        }
                        else
                        {
                            bool1 = true;
                        }
                    }
                    else if (_avg && IsMesec == Visibility.Visible && IsGodina == Visibility.Visible)
                    {
                        if (listMonthItems.Count > 0)
                        {
                            foreach (var l in listMonthItems)
                            {
                                sum += l.PAvg;
                                counter++;
                            }

                            _chartValuesMonth[0] = sum / counter;
                        }
                        else
                        {
                            bool2 = true;
                        }
                    }
                    else if (_avg && IsMesec == Visibility.Hidden && IsGodina == Visibility.Visible)
                    {
                        if (listYearItems.Count > 0)
                        {
                            foreach (var l in listYearItems)
                            {
                                sum += l.PAvg;
                                counter++;
                            }

                            _chartValuesYear[0] = sum / counter;
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
