﻿using System;
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
    /// Interaction logic for DERDashboardUserControl.xaml
    /// </summary>
    public partial class DERDashboardUserControl : UserControl
    {
        public DERDashboardUserControl()
        {
            InitializeComponent();

            DataContext = new DERDashboardUserControlViewModel(this);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window w = new ManualCommandingWindow();
            w.Show();
        }
    }
}
