using CalculationEngineServiceCommon;
using DERMSCommon.SCADACommon;
using DERMSCommon.WeatherForecast;
using UI.Resources.MediatorPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UI.Communication
{
    public class SendSCADADataToUI : ISendSCADADataToUI
    {
        public void SendScadaDataToUI(List<DataPoint> data)
        {
            Mediator.NotifyColleagues("SCADADataPoint", data);

        }
    }
}
