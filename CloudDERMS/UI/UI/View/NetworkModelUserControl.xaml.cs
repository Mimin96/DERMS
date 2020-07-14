using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI.ViewModel;

namespace UI.View
{
    /// <summary>
    /// Interaction logic for NetworkModelUserControl.xaml
    /// </summary>
    public partial class NetworkModelUserControl : UserControl
    {
        Button _selectedTreeButton;

        public NetworkModelUserControl()
        {
             InitializeComponent();

            _selectedTreeButton = new Button();
            DataContext = new NetworkModelUserControlViewModel();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //((NetworkModelUserControlViewModel)DataContext).GidInput;
        }

        private void SelectedElementFromTree(object sender, RoutedEventArgs e)
        {
            var converter = new System.Windows.Media.BrushConverter();

            _selectedTreeButton.Background = (Brush)converter.ConvertFromString("#FF303030");

            _selectedTreeButton = (Button)sender;
            _selectedTreeButton.Background = (Brush)converter.ConvertFromString("#1c1c1c");
        }
    }
}
