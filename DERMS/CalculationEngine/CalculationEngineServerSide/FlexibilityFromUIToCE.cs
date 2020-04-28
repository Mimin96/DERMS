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
			Dictionary<long, double> keyValues = new Dictionary<long, double>();
			keyValues[GID] = NormalOpen ? 1 : 0;

			ClientSideCE.Instance.ProxyScadaListOfGenerators.SendListOfGenerators(keyValues);
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
