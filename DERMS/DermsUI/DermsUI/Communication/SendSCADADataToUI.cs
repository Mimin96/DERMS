using CalculationEngineServiceCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using DermsUI.MediatorPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DermsUI.Communication
{
    public class SendSCADADataToUI : ISendSCADADataToUI
    {
        public void SendDataUI(Dictionary<long, DerForecastDayAhead> data)
        {
            //Mediator.NotifyColleagues("SCADAData", data);
            MessageBox.Show("Data count: " + data.Count, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
