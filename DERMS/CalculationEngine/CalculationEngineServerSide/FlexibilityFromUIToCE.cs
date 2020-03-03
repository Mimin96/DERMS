using CalculationEngineServiceCommon;
using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService
{
    [DataContract]
    public class FlexibilityFromUIToCE : IFlexibilityFromUIToCE
    {
        public void UpdateFlexibilityFromUIToCE(double valueKW, string incOrDec)
        {
            // POZOVI METODU ZA RACUNANJE FLEXIBILITY
            DataToUI data = new DataToUI();
            data.Flexibility = valueKW;
            CalculationEngineCache.Instance.CalculateNewFlexibility(data);
        }
    }
}
