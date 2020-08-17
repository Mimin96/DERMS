﻿using dCom.Exceptions;
using dCom.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace dCom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                DataContext = new MainViewModel();
                this.Closed += Window_Closed;
            }
            catch (ConfigurationException confEx)
            {
                MessageBox.Show($"{confEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error occured!{Environment.NewLine}Stack trace:{Environment.NewLine}{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as IDisposable).Dispose();
        }

        private void CheckBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonExit.Background = Brushes.Red;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonExit.Background = Brushes.Transparent;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}