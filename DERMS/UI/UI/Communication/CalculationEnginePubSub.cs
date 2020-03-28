using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.WeatherForecast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Resources.MediatorPattern;

namespace UI.Communication
{
	public class CalculationEnginePubSub : ICalculationEnginePubSub
	{
		public void SendScadaDataToUI(DataToUI data)
		{
			if (data.Topic.Equals((int)Enums.Topics.DerForecastDayAhead))
			{
				Mediator.NotifyColleagues("DerForecastDayAhead", data);
			}
			else if (data.Topic.Equals((int)Enums.Topics.Flexibility))
			{
				Mediator.NotifyColleagues("Flexibility", data);
			}
			else if (data.Topic.Equals((int)Enums.Topics.NetworkModelTreeClass))
			{
				Mediator.NotifyColleagues("NetworkModelTreeClass", data);
			}
		}
	}
}
