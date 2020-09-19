using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DERMSCommon.DataModel.Core;
using DERMSCommon.WeatherForecast;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECalculationMicroservice
{
    public class IslandCalculationsService : IIslandCalculations
    { 
        public async Task GeneratorOff(long generatorGid, Dictionary<long, DerForecastDayAhead> prod)
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            foreach (long gid in networkModel.Keys)
            {
                IdentifiedObject io = networkModel[gid];
                var type = io.GetType();

                if (type.Name.Equals("Substation"))
                {
                    Substation substation = (Substation)networkModel[gid];
                    if (substation.Equipments.Contains(generatorGid))
                    {
                        prod[gid].Production -= prod[generatorGid].Production;
                        //
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddDerForecastDayAhead(gid, prod[gid])).Wait();
                        //
                        SubGeographicalRegion subgr = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                        prod[subgr.GlobalId].Production -= prod[generatorGid].Production;
                        //
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddDerForecastDayAhead(subgr.GlobalId, prod[subgr.GlobalId])).Wait();
                        //
                        prod[subgr.GeoReg].Production -= prod[generatorGid].Production;
                        //
                        transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddDerForecastDayAhead(subgr.GeoReg, prod[subgr.GeoReg])).Wait();
                        //

                    }
                }
            }
        }
        public async Task GeneratorOn(long generatorGid, Dictionary<long, DerForecastDayAhead> prod)
        {
            Dictionary<long, IdentifiedObject> networkModel = new Dictionary<long, IdentifiedObject>();
            CloudClient<ICache> transactionCoordinator = new CloudClient<ICache>
            (
              serviceUri: new Uri("fabric:/CalculateEngineApp/CECommandMicroservice"),
              partitionKey: new ServicePartitionKey(0),
              clientBinding: WcfUtility.CreateTcpClientBinding(),
              listenerName: "CECacheServiceListener"
            );

            networkModel = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetNMSModel()).Result;
            foreach (long gid in networkModel.Keys)
            {
                IdentifiedObject io = networkModel[gid];
                var type = io.GetType();

                if (type.Name.Equals("Substation"))
                {
                    Substation substation = (Substation)networkModel[gid];
                    if (substation.Equipments.Contains(generatorGid))
                    {
                        prod[gid].Production += prod[generatorGid].Production;
                        SubGeographicalRegion subgr = (SubGeographicalRegion)networkModel[substation.SubGeoReg];
                        prod[subgr.GlobalId].Production += prod[generatorGid].Production;
                        prod[subgr.GeoReg].Production += prod[generatorGid].Production;

                    }
                }
            }
        }
    }
}
