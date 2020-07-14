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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            GIS.Background = new SolidColorBrush(Color.FromRgb(72, 74, 72));
            ((MenuViewModel)Menu.DataContext).SelectedMenuItem = GIS;
            ((MenuViewModel)Menu.DataContext).UserControlPresenter = new GISUserControl();

            ((MenuViewModel)Menu.DataContext).LoadingWindow();
        }

        #region Window Close, Resize, Minimize, Drag
        private void WindowClose(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void WindowResize(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Normal)
            {
                WindowSize.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowSize.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
                WindowState = WindowState.Normal;
            }
        }

        private void WindowMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion
    }
}
