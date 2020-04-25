using CalculationEngineServiceCommon;
using DERMSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static DERMSCommon.Enums;

namespace CalculationEngineService
{
    [DataContract]
    public class FlexibilityFromUIToCE : IFlexibilityFromUIToCE
    {
		public void ChangeBreakerStatus(long GID, bool NormalOpen)
		{
			throw new NotImplementedException();
		}

		public void UpdateFlexibilityFromUIToCE(double valueKW, FlexibilityIncDec incOrDec, long gid)
		{
			// POZOVI METODU ZA RACUNANJE FLEXIBILITY
			DataToUI data = new DataToUI();
			data.Flexibility = valueKW;
			data.Gid = gid;
			data.FlexibilityIncDec = incOrDec;
			CalculationEngineCache.Instance.CalculateNewFlexibility(data);
		}
	}
}
