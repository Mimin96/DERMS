using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using UI.View;
using System.Windows;
using UI.Resources;

namespace UI.ViewModel
{
    public class MenuViewModel : BindableBase
    {
        #region Variables
        private UserControl _userControlPresenter;
        private Button _selectedMenuItem;
        private RelayCommand<object> _menuSelectCommand;
        #endregion

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
