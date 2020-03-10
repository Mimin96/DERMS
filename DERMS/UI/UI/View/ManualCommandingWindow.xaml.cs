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

namespace UI.View
{
	/// <summary>
	/// Interaction logic for ManualCommandingWindow.xaml
	/// </summary>
	public partial class ManualCommandingWindow : Window
	{
		public ManualCommandingWindow()
		{
			InitializeComponent();
			DataContext = new ManualCommandingViewModel();

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (Increase.IsChecked == true)
			{
				((ManualCommandingViewModel)DataContext).Command(Double.Parse(ValueKW.Text), "Increase");
			}
			else
			{
				((ManualCommandingViewModel)DataContext).Command(Double.Parse(ValueKW.Text), "Decrease");
			}

			((ManualCommandingViewModel)DataContext).CloseConnection();

            Close();
		}
	}
}
