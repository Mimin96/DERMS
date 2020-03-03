using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Model;
using UI.Resources;

namespace UI.ViewModel
{
    public class SignalsSummaryUserControlViewModel : BindableBase
    {
        #region Variables
        private double _minHeightFilter;
        private Visibility _filterVisibility;
        private RelayCommand<object> _filterOnOff;
        private RelayCommand<object> _applyFilterCommand;
        private ObservableCollection<DataPoint> _points;
        private ObservableCollection<DataPoint> _allPoints;
        private DataPoint _selectedDataItem;
        private ICommand _selectedPointCommand;
        #endregion

        public SignalsSummaryUserControlViewModel()
        {
            SignalsSummaryFilter = new SignalsSummaryFilter();
            FilterVisibility = Visibility.Collapsed;

            FilterType = new List<string>() { "", PointType.ANALOG_INPUT.ToString(), PointType.ANALOG_OUTPUT.ToString(), PointType.DIGITAL_INPUT.ToString(), PointType.DIGITAL_OUTPUT.ToString() };
            FilterAlarm = new List<string>() { "", AlarmType.NO_ALARM.ToString(), AlarmType.ABNORMAL_VALUE.ToString(), AlarmType.REASONABILITY_FAILURE.ToString(), AlarmType.LOW_ALARM.ToString(), AlarmType.HIGH_ALARM.ToString() };
            MinHeightFilter = 20;

            Test();
        }

        #region Properties
        public SignalsSummaryFilter SignalsSummaryFilter
        {
            get;
            set;
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
        public ICommand FilterOnOffCommand
        {
            get
            {
                if (_filterOnOff == null)
                {
                    _filterOnOff = new RelayCommand<object>(FilterOnOff);
                }

                return _filterOnOff;
            }
        }
        public Visibility FilterVisibility
        {
            get { return _filterVisibility; }
            set { _filterVisibility = value; OnPropertyChanged("FilterVisibility"); }
        }
        public List<string> FilterType
        {
            get;
            set;
        }
        public List<string> FilterAlarm
        {
            get;
            set;
        }
        public double MinHeightFilter { get { return _minHeightFilter; } set { _minHeightFilter = value; OnPropertyChanged("MinHeightFilter"); } }
        public ObservableCollection<DataPoint> Points { get { return _points; } set { _points = value; OnPropertyChanged("Points"); } }
        public DataPoint SelectedDataItem
        {
            get
            {
                return _selectedDataItem;
            }
            set
            {
                _selectedDataItem = value;
            }
        }
        public ICommand SelectedPointCommand
        {
            get
            {
                if (_selectedPointCommand == null)
                {
                    _selectedPointCommand = new RelayCommand<object>(ShowCommanding);
                }

                return _selectedPointCommand;
            }
        }
        #endregion

        #region Public Method
        public void OnFocusName(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Name")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusName(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Name")
            {
                textBox.Text = "Name";
                SignalsSummaryFilter.Name = "Name";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        public void OnFocusAddress(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Address")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusAddress(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Address")
            {
                textBox.Text = "Address";
                SignalsSummaryFilter.Address = "Address";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        public void OnFocusValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Value")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Value")
            {
                textBox.Text = "Value";
                SignalsSummaryFilter.Value = "Value";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        public void OnFocusRawValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Raw Value")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusRawValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Raw Value")
            {
                textBox.Text = "Raw Value";
                SignalsSummaryFilter.RawValue = "Raw Value";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        public void OnFocusGID(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "GID")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusGID(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "GID")
            {
                textBox.Text = "GID";
                SignalsSummaryFilter.GID = "GID";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region Private Method
        private void Test()
        {
            _allPoints = new ObservableCollection<DataPoint>();
            _allPoints.Add(new DataPoint(11, PointType.ANALOG_INPUT, 111, DateTime.Now, "ffd", "24", 23, AlarmType.ABNORMAL_VALUE) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff33cc")) });
            _allPoints.Add(new DataPoint(12, PointType.DIGITAL_INPUT, 112, DateTime.Now.AddDays(1), "43", "sda", 24, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(13, PointType.ANALOG_INPUT, 113, DateTime.Now, "ddsfd", "24", 253, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(14, PointType.DIGITAL_OUTPUT, 114, DateTime.Now, "dsdfsd", "32", 43, AlarmType.HIGH_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#cc0000")) });
            _allPoints.Add(new DataPoint(15, PointType.ANALOG_INPUT, 115, DateTime.Now.AddDays(4), "12", "sda", 23, AlarmType.LOW_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff3300")) });
            _allPoints.Add(new DataPoint(16, PointType.ANALOG_OUTPUT, 333, DateTime.Now, "dffad", "43", 23, AlarmType.REASONABILITY_FAILURE) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff9966")) });
            _allPoints.Add(new DataPoint(17, PointType.ANALOG_INPUT, 116, DateTime.Now.AddDays(2), "87", "sda", 12, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(18, PointType.ANALOG_INPUT, 117, DateTime.Now, "ddadaf", "11", 223, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(19, PointType.ANALOG_INPUT, 118, DateTime.Now, "ddfgdfg", "222", 213, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(21, PointType.ANALOG_OUTPUT, 191, DateTime.Now, "ddfdg", "134", 233, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(22, PointType.ANALOG_OUTPUT, 171, DateTime.Now, "dddfg", "532", 243, AlarmType.NO_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(23, PointType.ANALOG_OUTPUT, 211, DateTime.Now.AddDays(3), "ddffr", "875", 253, AlarmType.HIGH_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = Brushes.Transparent });
            _allPoints.Add(new DataPoint(24, PointType.ANALOG_INPUT, 311, DateTime.Now, "ddsdfs", "908", 263, AlarmType.LOW_ALARM) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff3300")) });
            _allPoints.Add(new DataPoint(25, PointType.ANALOG_INPUT, 411, DateTime.Now.AddDays(5), "ddaa", "432", 273, AlarmType.REASONABILITY_FAILURE) { AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive, AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff9966")) });

            Points = new ObservableCollection<DataPoint>(_allPoints);
        }
        private void ShowCommanding(object obj)
        {

        }
        private void FilterOnOff(object obj)
        {
            if (FilterVisibility == Visibility.Collapsed)
            {
                FilterVisibility = Visibility.Visible;
                MinHeightFilter = 100;
            }
            else
            {
                FilterVisibility = Visibility.Collapsed;
                MinHeightFilter = 20;
            }
        }
        private void ApplyFilter(object obj)
        {
            bool isFilterApplied = false;
            List<DataPoint> toShow = new List<DataPoint>();
            List<DataPoint> helper1, helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.Name.Trim() != "Name")
            {
                isFilterApplied = true;
                toShow.AddRange(_allPoints.Where(x => x.Name.Trim().Contains(SignalsSummaryFilter.Name.Trim())).ToList());
            }

            if (SignalsSummaryFilter.Value.Trim() != "Value")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Value.Trim() == SignalsSummaryFilter.Value.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.RawValue.Trim() != "Raw Value")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.RawValue.ToString().Trim() == SignalsSummaryFilter.RawValue.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.Address.Trim() != "Address")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Address.ToString().Trim() == SignalsSummaryFilter.Address.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.GID.Trim() != "GID")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Gid.ToString().Trim() == SignalsSummaryFilter.GID.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.SelectedFilterAlarm.Trim() != "")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Alarm.ToString().Trim() == SignalsSummaryFilter.SelectedFilterAlarm.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.SelectedFilterType.Trim() != "")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Type.ToString().Trim() == SignalsSummaryFilter.SelectedFilterType.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);
            }

            helper2 = new List<DataPoint>();

            if (SignalsSummaryFilter.FilterByTime)
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.Timestamp.CompareTo(SignalsSummaryFilter.From) >= 0 && x.Timestamp.CompareTo(SignalsSummaryFilter.To) <= 0).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.Gid == dataPoint.Gid))
                    {
                        helper2.Add(dataPoint);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<DataPoint>(helper1);
                else
                    toShow = new List<DataPoint>(helper2);

                helper2 = new List<DataPoint>();
            }

            if (isFilterApplied)
                Points = new ObservableCollection<DataPoint>(toShow);
            else
                Points = new ObservableCollection<DataPoint>(_allPoints);
        }
        #endregion
    }
}
