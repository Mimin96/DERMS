using CalculationEngineServiceCommon;
using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Wires;
using DERMSCommon.WeatherForecast;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static DERMSCommon.Enums;

namespace CECommandMicroservice
{
    [DataContract]
    public class FlexibilityFromUIToCEService : IFlexibilityFromUIToCE
    {
        public async Task ChangeBreakerStatus(long GID, bool NormalOpen)
        {
            Dictionary<long, double> keyValues = new Dictionary<long, double>();
            keyValues[GID] = NormalOpen ? 1 : 0;

            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );
            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            Breaker breaker = (Breaker)networkModel[GID];
            Dictionary<long, DerForecastDayAhead> prod = new Dictionary<long, DerForecastDayAhead>();
            Dictionary<long, DerForecastDayAhead> TempProductionCached = new Dictionary<long, DerForecastDayAhead>();
            TempProductionCached = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTempProductionCached()).Result; //Returns nothing
            prod = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetAllDerForecastDayAhead()).Result;
            CloudClient<IIslandCalculations> transactionCoordinatorIsland = new CloudClient<IIslandCalculations>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "IslandCalculationsListener"
            );
            Dictionary<int, List<long>> tempTurnedOffGen = new Dictionary<int, List<long>>();
            List<long> TurnedOffGenerators = new List<long>();
            Dictionary<int, List<long>> tempTurnedOnGen = new Dictionary<int, List<long>>();
            List<long> TurnedOnGenerators = new List<long>();

            tempTurnedOnGen = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTurnedOnGenerators()).Result;
            tempTurnedOffGen = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetTurnedOffGenerators()).Result;
            if(tempTurnedOffGen.Count >0)
                TurnedOffGenerators = tempTurnedOffGen[0];/*Ovde pukne*/
            if (tempTurnedOnGen.Count > 0)
                TurnedOnGenerators = tempTurnedOnGen[0];
            if (NormalOpen)
            {
                foreach (long generatorGid in breaker.Generators)
                {
                    if (!TurnedOffGenerators.Contains(generatorGid))
                    {
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToTurnedOffGenerators(generatorGid)).Wait();
                        transactionCoordinatorIsland.InvokeWithRetryAsync(client => client.Channel.GeneratorOff(generatorGid, prod)).Wait();
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToTempProductionCached(generatorGid, prod[generatorGid])).Wait();
                        prod.Remove(generatorGid);
                        if (TurnedOnGenerators.Contains(generatorGid))
                            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTurnedOnGenerators(generatorGid)).Wait();
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead()).Wait();
                    }
                }
            }
            else
            {
                foreach (long generatorGid in breaker.Generators)
                {
                    if (TurnedOffGenerators.Contains(generatorGid))
                    {
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTurnedOffGenerators(generatorGid)).Wait();

                        prod.Add(generatorGid, TempProductionCached[generatorGid]);
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.RemoveFromTempProductionCached(generatorGid)).Wait();
                        transactionCoordinatorIsland.InvokeWithRetryAsync(client => client.Channel.GeneratorOn(generatorGid, prod)).Wait();
                        if (!TurnedOnGenerators.Contains(generatorGid))
                            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddToTurnedOnGenerators(generatorGid)).Wait();
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.SendDerForecastDayAhead()).Wait();
                    }
                }
            }
            //KAD SE URADI ClientSideCE RESITI OVU LINIJU
            CloudClient<ISendListOfGeneratorsToScada> transactionCoordinatorScada = new CloudClient<ISendListOfGeneratorsToScada>
            (
              serviceUri: new Uri("fabric:/SCADAApp/SCADACommandMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "SCADACommandingMicroserviceListener"
            );
            transactionCoordinatorScada.InvokeWithRetryAsync(client => client.Channel.SendListOfGenerators(keyValues)).Wait();

            //ClientSideCE.Instance.ProxyScadaListOfGenerators.SendListOfGenerators(keyValues);
        }

        public async Task UpdateFlexibilityFromUIToCE(double valueKW, FlexibilityIncDec incOrDec, long gid)
        {
            // POZOVI METODU ZA RACUNANJE FLEXIBILITY
            DataToUI data = new DataToUI();
            data.Flexibility = valueKW;
            data.Gid = gid;
            data.FlexibilityIncDec = incOrDec;

            CloudClient<IDERFlexibility> derFlexibility = new CloudClient<IDERFlexibility>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECalculationMicroservice"),
              partitionKey: new ServicePartitionKey(0), /*CJN*/
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "DERFlexibilityListener"
            );

            derFlexibility.InvokeWithRetryAsync(client => client.Channel.CalculateNewFlexibility(data)).Wait();

        }
    }
}
