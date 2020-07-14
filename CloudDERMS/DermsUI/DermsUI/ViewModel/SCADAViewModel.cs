using DERMSCommon.SCADACommon;
using DermsUI.MediatorPattern;
using DermsUI.Resources;
using DermsUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DermsUI.ViewModel
{
    public class SCADAViewModel:BindableBase
    {
        private DataPoint selectedDataItem;

        #region Properties
        public ObservableCollection<DataPoint> Points { get; set; }
        public DataPoint SelectedDataItem
        {
            get
            {
                return selectedDataItem;
            }
            set
            {
                selectedDataItem = value;
            }
        }
        public bool CanExecute { get { return true; } }
        #endregion
        #region Commanding
        private ICommand _selectedPointCommand;
        public ICommand SelectedPointCommand { get { return _selectedPointCommand ?? (_selectedPointCommand = new CommandHandler(() => ShowCommanding(), () => CanExecute)); }  }
        public void ShowCommanding()
        {
            CommandingWindow commandingWindow = new CommandingWindow();
            commandingWindow.DataContext = new CommandingWindowViewModel(SelectedDataItem);
            commandingWindow.ShowDialog();
        }
        #endregion

        public SCADAViewModel()
        {
            Points = new ObservableCollection<DataPoint>();
            Mediator.Register("AllSignalUpdate", OnChange);
            Mediator.NotifyColleagues("GetAllSignals", true);
        }

        #region Mediator
        public void OnChange(object parameter)
        {
            List<DataPoint> newPoints = (List<DataPoint>)parameter;

            foreach (DataPoint dataPointItem in newPoints)
            {
                DataPoint item = Points.Where(x => x.Gid == dataPointItem.Gid).FirstOrDefault();

                if (item == null)
                {
                    Points.Add(dataPointItem);
                }
                else
                {
                    Points.Remove(item);
                    Points.Add(dataPointItem);
                }
            }
            Points = new ObservableCollection<DataPoint>(Points.ToList().OrderBy(x => x.Address));
            OnPropertyChanged("Points");
        }
        #endregion
    }
}
