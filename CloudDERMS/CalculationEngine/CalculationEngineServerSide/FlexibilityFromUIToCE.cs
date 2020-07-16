using CalculationEngineServiceCommon;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.WeatherForecast;
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

            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            networkModel = CalculationEngineCache.Instance.GetNMSModel();
            Breaker breaker = (Breaker)networkModel[GID];
            Dictionary<long, DerForecastDayAhead> prod = new Dictionary<long, DerForecastDayAhead>();
            prod = CalculationEngineCache.Instance.GetAllDerForecastDayAhead();
            IslandCalculations islandCalculations = new IslandCalculations();
            if (NormalOpen)
            {
                if (!CalculationEngineCache.Instance.TurnedOffGenerators.Contains(21474836483))
                {
                    CalculationEngineCache.Instance.TurnedOffGenerators.Add(21474836483);
                    islandCalculations.GeneratorOff(21474836483, prod);
                    CalculationEngineCache.Instance.TempProductionCached.Add(21474836483, prod[21474836483]);
                    prod.Remove(21474836483);
                    if (CalculationEngineCache.Instance.TurnedOnGenerators.Contains(21474836483))
                        CalculationEngineCache.Instance.TurnedOnGenerators.Remove(21474836483);
                    CalculationEngineCache.Instance.SendDerForecastDayAhead();
                }
            }
            else
            {
                if (CalculationEngineCache.Instance.TurnedOffGenerators.Contains(21474836483))
                {
                    CalculationEngineCache.Instance.TurnedOffGenerators.Remove(21474836483);
                    
                    prod.Add(21474836483,CalculationEngineCache.Instance.TempProductionCached[21474836483]);
                    CalculationEngineCache.Instance.TempProductionCached.Remove(21474836483);
                    islandCalculations.GeneratorOn(21474836483, prod);
                    if (!CalculationEngineCache.Instance.TurnedOnGenerators.Contains(21474836483))
                        CalculationEngineCache.Instance.TurnedOnGenerators.Add(21474836483);
                    CalculationEngineCache.Instance.SendDerForecastDayAhead();
                }
            }
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
