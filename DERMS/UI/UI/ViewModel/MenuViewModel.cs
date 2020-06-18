using DERMSCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.UIModel;
using DERMSCommon.UIModel.ThreeViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Communication;
using UI.Resources;
using UI.Resources.MediatorPattern;
using UI.View;
using DERMSCommon.DataModel.Core;
using FTN.Common;

namespace UI.ViewModel
{
    public class MenuViewModel : BindableBase
    {
        #region Variables
        private LoadingWindow loadingWindow;
        private TreeNode<NodeData> _tree;
        private List<NetworkModelTreeClass> _networkModelTreeClass;
        private UserControl _userControlPresenter;
        private Button _selectedMenuItem;
        private RelayCommand<object> _menuSelectCommand;
        //private CommunicationProxy _proxy;
        private ClientSideProxy _clientSideProxy;
        //private CalculationEnginePubSub _calculationEnginePubSub;
        private List<DataPoint> _SCADAData;
        private object _dERDashboardDerForecastDayAhead;
        private object _dERDashboardFlexibility;
        #endregion

        public MenuViewModel()
        {
            Mediator.Register("SCADADataPoint", GetSCADAData);
            Mediator.Register("NMSNetworkModelData", GetNetworkModelFromProxy);
            Mediator.Register("NetworkModelTreeClass", NetworkModelTreeClassChangedMenu);
            Mediator.Register("Flexibility", DERDashboardFlexibility);
            Mediator.Register("DerForecastDayAhead", DERDashboardDerForecastDayAhead);

            _clientSideProxy = ClientSideProxy.Instance;
            
            _clientSideProxy.Subscribe((int)Enums.Topics.DerForecastDayAhead);
            _clientSideProxy.Subscribe((int)Enums.Topics.NetworkModelTreeClass);

            _clientSideProxy.Subscribe((int)Enums.Topics.NetworkModelTreeClass_NodeData);
            _clientSideProxy.Subscribe((int)Enums.Topics.DataPoints);

            _clientSideProxy.Subscribe((int)Enums.Topics.Flexibility);
            //_proxy = new CommunicationProxy();
            //_proxy.Open();

            Logger.Log("UI is started.", Enums.Component.UI, Enums.LogLevel.Info);
        }

        #region Properties
        public UserControl UserControlPresenter
        {
            get
            {
                return _userControlPresenter;
            }
            set
            {
                _userControlPresenter = value;
                OnPropertyChanged("UserControlPresenter");
            }
        }
        public Button SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { _selectedMenuItem = value; }
        }
        public ICommand MenuSelectCommand
        {
            get
            {
                if (_menuSelectCommand == null)
                {
                    _menuSelectCommand = new RelayCommand<object>(ExecuteMenuSelectCommand);
                }

                return _menuSelectCommand;
            }
        }
        #endregion

        #region Public Methods
        private void GetSCADAData(object parameter)
        {
            List<DataPoint> pom = (List<DataPoint>)parameter;
            if (_SCADAData == null)
            {
                _SCADAData = new List<DataPoint>(pom);

            }
            else
            {
                foreach (DataPoint data in pom)
                {
                    DataPoint dp = _SCADAData.Where(x => x.Gid == data.Gid).FirstOrDefault();

                    if (dp == null)
                    {
                        _SCADAData.Add(data);
                        continue;
                    }

                    dp.Alarm = data.Alarm;
                    dp.RawValue = data.RawValue;
                    dp.Timestamp = data.Timestamp;
                    dp.Value = data.Value;
                }
            }

            try
            {
                ((SignalsSummaryUserControlViewModel)UserControlPresenter.DataContext).GetSCADAData(_SCADAData);
            }
            catch
            {
            }
        }
        private void GetNetworkModelFromProxy(object parameter)
        {
            if (loadingWindow != null)
            {
                loadingWindow.Close();
                loadingWindow = null;
            }

            List<object> obj = (List<object>)parameter;
            _tree = (TreeNode<NodeData>)obj[0];
            _networkModelTreeClass = (List<NetworkModelTreeClass>)obj[1];

            if (UserControlPresenter.GetType().Name == "GISUserControl")
                ((GISUserControlViewModel)UserControlPresenter.DataContext).Tree = _tree;
            //SetUserContro("GIS");
        }

        public void DERDashboardFlexibility(object parameter) 
        {
            _dERDashboardFlexibility = parameter;

            if (UserControlPresenter.GetType().Name == "DERDashboardUserControl")
                ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).DERDashboardFlexibility(parameter);
        }

        public void DERDashboardDerForecastDayAhead(object parameter) 
        {
            _dERDashboardDerForecastDayAhead = parameter;

            if (UserControlPresenter.GetType().Name == "DERDashboardUserControl")
                ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).DERDashboardDerForecastDayAhead(parameter);
        }
        public void NetworkModelTreeClassChangedMenu(object parameter)
        {
            _networkModelTreeClass = ((DataToUI)parameter).NetworkModelTreeClass;

			if (_tree != null)
			{
				List<NetworkModelTreeClass> _networkModelTreeClass = ((DataToUI)parameter).NetworkModelTreeClass;

				foreach (NetworkModelTreeClass io in _networkModelTreeClass)
				{
					foreach (GeographicalRegionTreeClass geographicalRegionTreeClass in io.GeographicalRegions)
					{
						foreach (GeographicalSubRegionTreeClass geographicalSubRegionTreeClass in geographicalRegionTreeClass.GeographicalSubRegions)
						{
							foreach (SubstationTreeClass substationTreeClass in geographicalSubRegionTreeClass.Substations)
							{
								foreach (SubstationElementTreeClass substationElementTreeClass in substationTreeClass.SubstationElements)
								{
									if (substationElementTreeClass.Type.Equals(DMSType.GENERATOR))
									{
										TreeNode<NodeData> nodeData = _tree.Where(x => x.Data.IdentifiedObject.GlobalId.Equals(substationElementTreeClass.GID)).FirstOrDefault();
										if (nodeData.Data.IdentifiedObject != null)
										{
											((Generator)nodeData.Data.IdentifiedObject).MinFlexibility = substationElementTreeClass.MinFlexibility;
											((Generator)nodeData.Data.IdentifiedObject).MaxFlexibility = substationElementTreeClass.MaxFlexibility;
										}
									}
								}
							}
						}
					}
				}
			}

			if (UserControlPresenter.GetType().Name == "DERDashboardUserControl")
            {
                ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).NetworkModel = _networkModelTreeClass;
            }

			if (UserControlPresenter.GetType().Name == "GISUserControl")
			{
				((GISUserControlViewModel)UserControlPresenter.DataContext).Tree = _tree; ;
			}
		}

        public void ExecuteMenuSelectCommand(object sender)
        {
            if (_selectedMenuItem != null)
            {
                _selectedMenuItem.Background = Brushes.Transparent;
            }

            System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
            button.Background = new SolidColorBrush(Color.FromRgb(72, 74, 72));
            SetUserContro(button.Name.ToString());

            _selectedMenuItem = button;
        }
        public void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            ((MenuItem)((FrameworkElement)sender).Parent).IsSubmenuOpen = false;

            if (_selectedMenuItem != null)
            {
                _selectedMenuItem.Background = Brushes.Transparent;
            }

            if (sender == null) return;

            Button button = (Button)sender;
            button.Background = new SolidColorBrush(Color.FromRgb(72, 74, 72));
            SetUserContro(button.Name.ToString());

            _selectedMenuItem = button;
        }
        public void LoadingWindow()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                loadingWindow = new LoadingWindow();
                loadingWindow.ShowDialog();
            });
        }
        #endregion

        #region Private Methods
        private void SetUserContro(string button)
        {
            switch (button)
            {
                case "GIS":
                    UserControlPresenter = new GISUserControl();
                    ((GISUserControlViewModel)UserControlPresenter.DataContext).Tree = _tree;
                    break;
                case "DERDashboard":
                    UserControlPresenter = new DERDashboardUserControl();
                    ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).Tree = _tree;
                    ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).NetworkModel = _networkModelTreeClass;
                    if(_dERDashboardFlexibility != null)
                        ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).DERDashboardFlexibility(_dERDashboardFlexibility);
                    ((DERDashboardUserControlViewModel)UserControlPresenter.DataContext).DERDashboardDerForecastDayAhead(_dERDashboardDerForecastDayAhead);
                    break;
                case "NetworkModel":
                    UserControlPresenter = new NetworkModelUserControl();
                    ((NetworkModelUserControlViewModel)UserControlPresenter.DataContext).Tree = _tree;
                    ((NetworkModelUserControlViewModel)UserControlPresenter.DataContext).NetworkModel = _networkModelTreeClass;
                    break;
                case "SignalsSummary":
                    UserControlPresenter = new SignalsSummaryUserControl();
                    if (_SCADAData != null)
                        ((SignalsSummaryUserControlViewModel)UserControlPresenter.DataContext).GetSCADAData(_SCADAData);
                    break;
                case "EventSummary":
                    UserControlPresenter = new EventSummaryUserControl();
                    break;
                case "History":
                    UserControlPresenter = new HistoryUserControl();
                    ((HistoryUserControlViewModel)UserControlPresenter.DataContext).NetworkModel = _networkModelTreeClass;
                    break;
                default:
                    MessageBox.Show("There was a problem while opening view. Try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
        private string GetParents(Object element, int parentLevel)
        {
            string returnValue = String.Format("[{0}] {1}", parentLevel, element.GetType());
            if (element is FrameworkElement)
            {
                if (((FrameworkElement)element).Parent != null)
                    returnValue += String.Format("{0}{1}",
                        Environment.NewLine, GetParents(((FrameworkElement)element).Parent, parentLevel + 1));
            }
            return returnValue;
        }
        #endregion
    }
}
