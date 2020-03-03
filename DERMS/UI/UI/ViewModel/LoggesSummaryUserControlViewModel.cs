using DERMSCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel;
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
using UI.Model;
using UI.Resources;

namespace UI.ViewModel
{
    public class LoggesSummaryUserControlViewModel : BindableBase
    {
        #region Variables
        private double _minHeightFilter;
        private Visibility _filterVisibility;
        private RelayCommand<object> _filterOnOff;
        private RelayCommand<object> _applyFilterCommand;
        private ObservableCollection<Log> _loggs;
        private ObservableCollection<Log> _allLoggs;
        #endregion

        public LoggesSummaryUserControlViewModel()
        {
            LoggesSummaryFilter = new LoggesSummaryFilter();
            FilterVisibility = Visibility.Collapsed;

            FilterComponent = new List<string>() { "", Enums.Component.UI.ToString(), Enums.Component.SCADA.ToString(), Enums.Component.NMS.ToString(), Enums.Component.TransactionCoordinator.ToString(), Enums.Component.CalculationEngine.ToString() };
            FilterLogLevel = new List<string>() { "", Enums.LogLevel.Error.ToString(), Enums.LogLevel.Fatal.ToString(), Enums.LogLevel.Info.ToString(), Enums.LogLevel.Warning.ToString() };
            MinHeightFilter = 20;

            ShowCommanding();
        }

        #region Properties
        public LoggesSummaryFilter LoggesSummaryFilter
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
        public List<string> FilterComponent
        {
            get;
            set;
        }
        public List<string> FilterLogLevel
        {
            get;
            set;
        }
        public double MinHeightFilter { get { return _minHeightFilter; } set { _minHeightFilter = value; OnPropertyChanged("MinHeightFilter"); } }
        public ObservableCollection<Log> Loggs { get { return _loggs; } set { _loggs = value; OnPropertyChanged("Loggs"); } }
        #endregion

        #region Public Method
        public void OnFocusMessage(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "Message")
            {
                textBox.FontStyle = FontStyles.Normal;
                textBox.Text = "";
            }
        }
        public void OnOffFocusMessage(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim() == "" || textBox.Text.Trim() == "Message")
            {
                textBox.Text = "Message";
                LoggesSummaryFilter.Message = "Message";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region Private Method
        private void ShowCommanding()
        {
            _allLoggs = new ObservableCollection<Log>();
            _allLoggs.Add(new Log("dasdd", Enums.Component.SCADA, Enums.LogLevel.Error, DateTime.Now));
            _allLoggs.Add(new Log("dasdd", Enums.Component.SCADA, Enums.LogLevel.Error, DateTime.Now.AddDays(7)));
            _allLoggs.Add(new Log("dasdd", Enums.Component.SCADA, Enums.LogLevel.Info, DateTime.Now));
            _allLoggs.Add(new Log("sdsdfg", Enums.Component.UI, Enums.LogLevel.Error, DateTime.Now));
            _allLoggs.Add(new Log("dasdd", Enums.Component.SCADA, Enums.LogLevel.Warning, DateTime.Now));
            _allLoggs.Add(new Log("ggfgfg", Enums.Component.NMS, Enums.LogLevel.Error, DateTime.Now));

            Loggs = new ObservableCollection<Log>(_allLoggs);
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
            List<Log> toShow = new List<Log>();
            List<Log> helper1, helper2 = new List<Log>();

            if (LoggesSummaryFilter.Message.Trim() != "Message")
            {
                isFilterApplied = true;
                toShow.AddRange(_allLoggs.Where(x => x.Message.Trim().Contains(LoggesSummaryFilter.Message.Trim())).ToList());
            }

            if (LoggesSummaryFilter.SelectedFilterComponent.Trim() != "")
            {
                isFilterApplied = true;
                helper1 = _allLoggs.Where(x => x.Component.ToString().Trim() == LoggesSummaryFilter.SelectedFilterComponent.Trim()).ToList();

                foreach (Log log in toShow)
                {
                    if (helper1.Exists(x => x.Message.Contains(log.Message) && x.LogLevel == log.LogLevel && x.Component == log.Component && x.DateTime.CompareTo(log.DateTime) == 0))
                    {
                        helper2.Add(log);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<Log>(helper1);
                else
                    toShow = new List<Log>(helper2);
            }

            helper2 = new List<Log>();

            if (LoggesSummaryFilter.SelectedFilterLogLevel.Trim() != "")
            {
                isFilterApplied = true;
                helper1 = _allLoggs.Where(x => x.LogLevel.ToString().Trim() == LoggesSummaryFilter.SelectedFilterLogLevel.Trim()).ToList();

                foreach (Log log in toShow)
                {
                    if (helper1.Exists(x => x.Message.Contains(log.Message) && x.LogLevel == log.LogLevel && x.Component == log.Component && x.DateTime.CompareTo(log.DateTime) == 0))
                    {
                        helper2.Add(log);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<Log>(helper1);
                else
                    toShow = new List<Log>(helper2);
            }

            helper2 = new List<Log>();

            if (LoggesSummaryFilter.FilterByTime)
            {
                isFilterApplied = true;
                helper1 = _allLoggs.Where(x => x.DateTime.CompareTo(LoggesSummaryFilter.From) >= 0 && x.DateTime.CompareTo(LoggesSummaryFilter.To) <= 0).ToList();

                foreach (Log log in toShow)
                {
                    if (helper1.Exists(x => x.Message.Contains(log.Message) && x.LogLevel == log.LogLevel && x.Component == log.Component && x.DateTime.CompareTo(log.DateTime) == 0))
                    {
                        helper2.Add(log);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<Log>(helper1);
                else
                    toShow = new List<Log>(helper2);

                helper2 = new List<Log>();
            }

            if (isFilterApplied)
                Loggs = new ObservableCollection<Log>(toShow);
            else
                Loggs = new ObservableCollection<Log>(_allLoggs);
        }
        #endregion
    }
}
