using CalculationEngineServiceCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UI.Communication
{
    public class SendEventsToUI : ISendEventsToUI
    {
        public void ReceiveEventsFromCE(string rec)
        {
            MessageBox.Show("test : " + rec);

        }
    }
}
