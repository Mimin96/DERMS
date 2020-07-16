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
                foreach (long generatorGid in breaker.Generators)
                {
                    if (!CalculationEngineCache.Instance.TurnedOffGenerators.Contains(generatorGid))
                    {
                        CalculationEngineCache.Instance.TurnedOffGenerators.Add(generatorGid);
                        islandCalculations.GeneratorOff(generatorGid, prod);
                        CalculationEngineCache.Instance.TempProductionCached.Add(generatorGid, prod[generatorGid]);
                        prod.Remove(generatorGid);
                        if (CalculationEngineCache.Instance.TurnedOnGenerators.Contains(generatorGid))
                            CalculationEngineCache.Instance.TurnedOnGenerators.Remove(generatorGid);
                        CalculationEngineCache.Instance.SendDerForecastDayAhead();
                    }
                }
            }
            else
            {
                foreach (long generatorGid in breaker.Generators)
                {
                    if (CalculationEngineCache.Instance.TurnedOffGenerators.Contains(generatorGid))
                    {
                        CalculationEngineCache.Instance.TurnedOffGenerators.Remove(generatorGid);

                        prod.Add(generatorGid, CalculationEngineCache.Instance.TempProductionCached[generatorGid]);
                        CalculationEngineCache.Instance.TempProductionCached.Remove(generatorGid);
                        islandCalculations.GeneratorOn(generatorGid, prod);
                        if (!CalculationEngineCache.Instance.TurnedOnGenerators.Contains(generatorGid))
                            CalculationEngineCache.Instance.TurnedOnGenerators.Add(generatorGid);
                        CalculationEngineCache.Instance.SendDerForecastDayAhead();
                    }
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
