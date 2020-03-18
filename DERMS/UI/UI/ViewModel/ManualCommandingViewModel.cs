using CalculationEngineServiceCommon;
using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DERMSCommon.Enums;

namespace UI.ViewModel
{
	public class ManualCommandingViewModel : BindableBase
	{
		#region Variables
		private IFlexibilityFromUIToCE ProxyCE { get; set; }
		private ChannelFactory<IFlexibilityFromUIToCE> factoryCE;
		#endregion

		public ManualCommandingViewModel()
		{
			Connect();
		}

		public void Command(double valueKW, FlexibilityIncDec incdec, long gid)
		{
			//ProxyCE.UpdateThroughUI(valueKW, incOrDec);
			try
			{
				ProxyCE.UpdateFlexibilityFromUIToCE(valueKW, incdec, gid);
			}
			catch (Exception e)
			{
				MessageBox.Show("ManualCommandingViewModel 34: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Connect()
		{
			try
			{
				NetTcpBinding binding = new NetTcpBinding();
				binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
				binding.CloseTimeout = System.TimeSpan.FromMinutes(20);
				binding.OpenTimeout = System.TimeSpan.FromMinutes(20);
				binding.ReceiveTimeout = System.TimeSpan.FromMinutes(20);
				binding.SendTimeout = System.TimeSpan.FromMinutes(20);
				binding.MaxBufferSize = 8000000;
				binding.MaxReceivedMessageSize = 8000000;
				binding.MaxBufferPoolSize = 8000000;
				factoryCE = new ChannelFactory<IFlexibilityFromUIToCE>(binding, new EndpointAddress("net.tcp://localhost:19011/IFlexibilityFromUIToCE"));
				ProxyCE = factoryCE.CreateChannel();
			}
			catch (Exception e)
			{
				MessageBox.Show("ManualCommandingViewModel 52: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void CloseConnection()
		{
			factoryCE.Abort();
		}
	}
}
