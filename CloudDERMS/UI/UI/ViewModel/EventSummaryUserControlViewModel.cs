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
    public class EventSummaryUserControlViewModel : BindableBase
    {
        #region Variables
        private double _minHeightFilter;
        private Visibility _filterVisibility;
        private RelayCommand<object> _filterOnOff;
        private RelayCommand<object> _applyFilterCommand;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Event> _allEvents;
       
        List<Event> listOfEvents = new List<Event>();
        EventsLogger events = new EventsLogger();
        #endregion

        public EventSummaryUserControlViewModel()
        {
            EventsSummaryFilter = new LoggesSummaryFilter();
            FilterComponent = new List<string>() { "", Enums.Component.UI.ToString(), Enums.Component.SCADA.ToString(), Enums.Component.NMS.ToString(), Enums.Component.TransactionCoordinator.ToString(), Enums.Component.CalculationEngine.ToString() };
            FilterVisibility = Visibility.Collapsed;

            MinHeightFilter = 20;

            ShowCommanding();
        }

        #region Properties
        public List<string> FilterComponent
        {
            get;
            set;
        }
        public LoggesSummaryFilter EventsSummaryFilter
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
        public double MinHeightFilter { get { return _minHeightFilter; } set { _minHeightFilter = value; OnPropertyChanged("MinHeightFilter"); } }
        public ObservableCollection<Event> Events { get { return _events; } set { _events = value; OnPropertyChanged("Events"); } }
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
                EventsSummaryFilter.Message = "Message";
                textBox.FontStyle = FontStyles.Italic;
            }
        }
        #endregion

        #region Private Method
        private void ShowCommanding()
        {
            //_allEvents = new ObservableCollection<Event>();
            //_allEvents.Add(new Event("dasdd", Enums.Component.SCADA, DateTime.Now));
            //_allEvents.Add(new Event("dasdd", Enums.Component.SCADA, DateTime.Now.AddDays(7)));
            //_allEvents.Add(new Event("dasdd", Enums.Component.SCADA, DateTime.Now));
            //_allEvents.Add(new Event("sdsdfg", Enums.Component.UI, DateTime.Now));
            //_allEvents.Add(new Event("dasdd", Enums.Component.SCADA, DateTime.Now));
            //_allEvents.Add(new Event("ggfgfg", Enums.Component.NMS, DateTime.Now));
            _allEvents = new ObservableCollection<Event>(events.ReadFromFile());

            Events = new ObservableCollection<Event>(_allEvents);
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
            List<Event> toShow = new List<Event>();
            List<Event> helper1, helper2 = new List<Event>();

            if (EventsSummaryFilter.Message.Trim() != "Message")
            {
                isFilterApplied = true;
                toShow.AddRange(_allEvents.Where(x => x.Message.Trim().Contains(EventsSummaryFilter.Message.Trim())).ToList());
            }

            if (EventsSummaryFilter.SelectedFilterComponent.Trim() != "")
            {
                isFilterApplied = true;
                helper1 = _allEvents.Where(x => x.Component.ToString().Trim() == EventsSummaryFilter.SelectedFilterComponent.Trim()).ToList();

                foreach (Event log in toShow)
                {
                    if (helper1.Exists(x => x.Message.Contains(log.Message) && x.Component == log.Component && x.DateTime.CompareTo(log.DateTime) == 0))
                    {
                        helper2.Add(log);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<Event>(helper1);
                else
                    toShow = new List<Event>(helper2);
            }

            helper2 = new List<Event>();

            if (EventsSummaryFilter.FilterByTime)
            {
                isFilterApplied = true;
                helper1 = _allEvents.Where(x => x.DateTime.CompareTo(EventsSummaryFilter.From) >= 0 && x.DateTime.CompareTo(EventsSummaryFilter.To) <= 0).ToList();

                foreach (Event log in toShow)
                {
                    if (helper1.Exists(x => x.Message.Contains(log.Message) && x.Component == log.Component && x.DateTime.CompareTo(log.DateTime) == 0))
                    {
                        helper2.Add(log);
                    }
                }

                if (toShow.Count == 0)
                    toShow = new List<Event>(helper1);
                else
                    toShow = new List<Event>(helper2);

                helper2 = new List<Event>();
            }

            if (isFilterApplied)
                Events = new ObservableCollection<Event>(toShow);
            else
                Events = new ObservableCollection<Event>(_allEvents);
        }
        #endregion
    }
}
