using DERMSCommon;
using DERMSCommon.UIModel.ThreeViewModel;
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
        #region Properties
        public TreeNode<NodeData> Tree { get; set; }
        private ClientSideProxy ClientSideProxy { get; set; }
        private CalculationEnginePubSub CalculationEnginePubSub { get; set; }
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
           
            Values1 = new ChartValues<ObservableValue>
            {
                new ObservableValue(3),
                new ObservableValue(4),
                new ObservableValue(6),
                new ObservableValue(3),
                new ObservableValue(2),
                new ObservableValue(6)
            };

            this.dERDashboardUserControl = dERDashboardUserControl;

            Mediator.Register("DerForecastDayAhead", DERDashboardDerForecastDayAhead);
            Mediator.Register("Flexibility", DERDashboardFlexibility);

            ClientSideProxy = new ClientSideProxy();
            CalculationEnginePubSub = new CalculationEnginePubSub();
            ClientSideProxy.StartServiceHost(CalculationEnginePubSub);
            ClientSideProxy.Subscribe((int)Enums.Topics.Flexibility);
        }

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            proxy = new CommunicationProxy();
            proxy.Open2();
            proxy.sendToCE.UpdateThroughUI(gid);
            Console.Beep();
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void SubstationCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void SubstationElementCommandExecute(long gid)
        {
            Console.Beep();
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
            // TREBA IMPLEMENTIRATI
        }
    }
}
