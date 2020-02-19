using CalculationEngineServiceCommon;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DermsUI.Communication
{
	public class CalculationEnginePubSub : ICalculationEnginePubSub
	{
		public void SendScadaDataToUI(Dictionary<long, DerForecastDayAhead> data)
		{
			MessageBox.Show("PROVERA: " + data.Count);
		}
	}
}
