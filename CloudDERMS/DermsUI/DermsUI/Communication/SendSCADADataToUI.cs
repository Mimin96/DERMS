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
        public void SendScadaDataToUI(List<DataPoint> data)
        {
            Mediator.NotifyColleagues("SCADAData", data);
        }
    }
}
