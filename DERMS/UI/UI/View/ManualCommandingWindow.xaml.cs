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
using System.Windows.Shapes;
using UI.ViewModel;
using static DERMSCommon.Enums;

namespace UI.View
{
	/// <summary>
	/// Interaction logic for ManualCommandingWindow.xaml
	/// </summary>
	public partial class ManualCommandingWindow : Window
	{
		private long Gid { get; set; }
		public ManualCommandingWindow(Double inc, Double dec, long gid)
		{
			InitializeComponent();
			DataContext = new ManualCommandingViewModel();
			IncreaseGauge.Value = inc;
			DecreaseGauge.Value = dec;
			Gid = gid;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (Increase.IsChecked == true)
			{
				((ManualCommandingViewModel)DataContext).Command(Double.Parse(ValueKW.Text), FlexibilityIncDec.Increase, Gid);
			}
			else
			{
				((ManualCommandingViewModel)DataContext).Command((-1) * Double.Parse(ValueKW.Text), FlexibilityIncDec.Decrease, Gid);
			}

			((ManualCommandingViewModel)DataContext).CloseConnection();

			Gid = 0;

			Close();
		}
	}
}
