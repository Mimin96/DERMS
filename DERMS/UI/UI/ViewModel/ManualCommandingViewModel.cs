using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.View;
using static DERMSCommon.Enums;

namespace UI.ViewModel
{
	public class ManualCommandingViewModel : BindableBase
	{
		#region Variables
		private IFlexibilityFromUIToCE ProxyCE { get; set; }
		private ChannelFactory<IFlexibilityFromUIToCE> factoryCE;
		private Double Inc { get; set; }
		private Double Dec { get; set; }
		#endregion

		public ManualCommandingViewModel(Double inc, Double dec)
		{
			Inc = inc;
			Dec = dec;
			Connect();
		}

		public void Command(double valueKW, FlexibilityIncDec incdec, long gid)
		{
			//ProxyCE.UpdateThroughUI(valueKW, incOrDec);
			try
			{
				bool canManualCommand = false;

				if (incdec.Equals(FlexibilityIncDec.Increase))
				{
					if (valueKW > Inc)
					{
						canManualCommand = false;
					}
					else
					{
						canManualCommand = true;
					}
				}
				else
				{
					if (-1 * valueKW > Dec)
					{
						canManualCommand = false;
					}
					else
					{
						canManualCommand = true;
					}
				}

				if (canManualCommand)
				{
					ProxyCE.UpdateFlexibilityFromUIToCE(valueKW, incdec, gid);

					Event e = new Event("Izvrsena je manuelna optimizacija", Enums.Component.CalculationEngine, DateTime.Now);
					EventsLogger el = new EventsLogger();
					el.WriteToFile(e);
				}
				else
				{
					ValidationForManualCommanding validationForManualCommanding = new ValidationForManualCommanding();
					validationForManualCommanding.ShowDialog();
				}
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
