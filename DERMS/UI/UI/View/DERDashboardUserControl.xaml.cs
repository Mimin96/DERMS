using LiveCharts.Wpf;
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
    /// Interaction logic for DERDashboardUserControl.xaml
    /// </summary>
    public partial class DERDashboardUserControl : UserControl
    {
        public DERDashboardUserControl()
        {
            InitializeComponent();
            var yAxis = new Axis { Separator = new LiveCharts.Wpf.Separator { StrokeThickness = 0.12 } };
            var sAxis = new Axis { Separator = new LiveCharts.Wpf.Separator { StrokeThickness = 0.1 } };
            cartesianChart.AxisY.Add(yAxis);
            cartesianChart.AxisX.Add(sAxis);

            DataContext = new DERDashboardUserControlViewModel(this);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window w = new ManualCommandingWindow(((DERDashboardUserControlViewModel)DataContext).GetIncreaseFlexibility(), ((DERDashboardUserControlViewModel)DataContext).GetDecreaseFlexibility(), ((DERDashboardUserControlViewModel)DataContext).CurrentSelectedGid);
            w.Show();
            CurrentConsumption.Text = (EnergySource.Value - ProductionFromGenerators.Value).ToString() + "kw/h";

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DERDashboardUserControlViewModel d = new DERDashboardUserControlViewModel(this);
            var energySourceValue = d.Optimization();
            EnergySource.Value = (int)energySourceValue;
            //CurrentConsumption.Text = (EnergySource.Value - ProductionFromGenerators.Value).ToString() + "kw/h";
        }
    }
}
