using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Communication;
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
			string text = "";

			try
			{
				bool canManualCommand = false;

				if (incdec.Equals(FlexibilityIncDec.Increase))
				{
					if (valueKW > 0)
					{
						if (valueKW > Inc)
						{
							if (Inc == 0)
								text = "It's impossible to increase flexibility any more.";
							else
								text = "Please enter value between\n0 and " + String.Format("{0:0.00}", Inc);
							canManualCommand = false;
						}
						else
						{
							canManualCommand = true;
						}
					}
					else
					{
						text = "Please enter positive value.";
						canManualCommand = false;
					}
				}
				else
				{
					if (valueKW < 0)
					{
						if (-1 * valueKW > Dec)
						{
							if (Dec == 0)
								text = "It's impossible to decrease flexibility any more.";
							else
								text = "Please enter value between\n0 and " + String.Format("{0:0.00}", Dec);
							canManualCommand = false;
						}
						else
						{
							canManualCommand = true;
						}
					}
					else
					{
						text = "Please enter positive value.";
						canManualCommand = false;
					}
				}
                

                if (canManualCommand)
				{
					ProxyCE.UpdateFlexibilityFromUIToCE(valueKW, incdec, gid);

					Event e = new Event("Manual optimization was performed. ", Enums.Component.CalculationEngine, DateTime.Now);
					EventsLogger el = new EventsLogger();

					el.WriteToFile(e);

				}
				else
				{
					PopUpWindow popUpWindow = new PopUpWindow(text);
					popUpWindow.ShowDialog();
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
