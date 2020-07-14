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
using UI.Resources.MediatorPattern;

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
        public void GetSCADAData(List<DataPoint> allPoints)
        {
            _allPoints = new ObservableCollection<DataPoint>(allPoints);

            foreach (DataPoint dp in _allPoints)
            {
                switch (dp.Alarm)
                {
                    case AlarmType.ABNORMAL_VALUE:
                        dp.AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive;
                        dp.AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff33cc"));
                        break;
                    case AlarmType.HIGH_ALARM:
                        dp.AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive;
                        dp.AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#cc0000"));
                        break;
                    case AlarmType.LOW_ALARM:
                        dp.AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive;
                        dp.AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff3300"));
                        break;
                    case AlarmType.REASONABILITY_FAILURE:
                        dp.AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive;
                        dp.AlarmImageColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff9966"));
                        break;
                    case AlarmType.NO_ALARM:
                        dp.AlarmImage = MaterialDesignThemes.Wpf.PackIconKind.NotificationsActive;
                        dp.AlarmImageColor = Brushes.Transparent;
                        break;
                }
            }

            Points = new ObservableCollection<DataPoint>(_allPoints);
        }
        public void OnFocusName(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Use")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusName(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Use")
            {
                textBox.Text = "Use";
                SignalsSummaryFilter.Name = "Use";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        public void OnFocusOwnersGID(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Owners GID")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusOwnersGID(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Owners GID")
            {
                textBox.Text = "Owners GID";
                SignalsSummaryFilter.Name = "Owners GID";
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
        public void OnFocusRawValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Value")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusRawValue(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Value")
            {
                textBox.Text = "Value";
                SignalsSummaryFilter.RawValue = "Value";
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

            if (SignalsSummaryFilter.Name.Trim() != "Use")
            {
                isFilterApplied = true;
                toShow.AddRange(_allPoints.Where(x => x.Name.Trim().Contains(SignalsSummaryFilter.Name.Trim())).ToList());
            }

            if (SignalsSummaryFilter.RawValue.Trim() != "Value")
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

            if (SignalsSummaryFilter.OwnersGID.Trim() != "Owners GID")
            {
                isFilterApplied = true;
                helper1 = _allPoints.Where(x => x.GidGeneratora.ToString().Trim() == SignalsSummaryFilter.OwnersGID.Trim()).ToList();

                foreach (DataPoint dataPoint in toShow)
                {
                    if (helper1.Exists(x => x.GidGeneratora == dataPoint.GidGeneratora))
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
