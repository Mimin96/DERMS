using DERMSCommon;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Communication;
using UI.Resources;
using UI.Resources.MediatorPattern;
using UI.View;

namespace UI.ViewModel
{
    public class MenuViewModel : BindableBase
    {
        #region Variables
        private UserControl _userControlPresenter;
        private Button _selectedMenuItem;
        private RelayCommand<object> _menuSelectCommand;
        private CommunicationProxy _proxy;
        private ClientSideProxy _clientSideProxy;
        private CalculationEnginePubSub _calculationEnginePubSub;
        #endregion

        public MenuViewModel() 
        {
            Mediator.Register("NMSNetworkModelData", GetNetworkModelFromProxy);

            _clientSideProxy = new ClientSideProxy();
            _calculationEnginePubSub = new CalculationEnginePubSub();
            _clientSideProxy.StartServiceHost(_calculationEnginePubSub);
            _clientSideProxy.Subscribe(1);

            _proxy = new CommunicationProxy();
            _proxy.Open();

            Logger.Log("UI is started.", DERMSCommon.Enums.Component.UI, DERMSCommon.Enums.LogLevel.Info);
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
        private void GetNetworkModelFromProxy(object parameter)
        {
            //TO DO
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
        #endregion

        #region Private Methods
        private void SetUserContro(string button)
        {
            switch (button)
            {
                case "GIS":
                    UserControlPresenter = new GISUserControl();
                    break;
                case "DERDashboard":
                    UserControlPresenter = new DERDashboardUserControl();
                    break;
                case "NetworkModel":
                    UserControlPresenter = new NetworkModelUserControl();
                    break;
                default:
                    MessageBox.Show("There was a problem while opening view. Try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
        #endregion
    }
}
