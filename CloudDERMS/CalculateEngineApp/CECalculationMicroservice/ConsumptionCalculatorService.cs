using CloudCommon.CalculateEngine;
using CloudCommon.CalculateEngine.Communication;
using DarkSkyApi.Models;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using DERMSCommon.WeatherForecast;
using FTN.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECalculationMicroservice
{
    public class ConsumptionCalculatorService : IConsumptionCalculator
    {
        public async Task Calculate(Dictionary<long, DerForecastDayAhead> derForcast, NetworkModelTransfer networkModel, Dictionary<long, DayAhead> subDayAhead, Dictionary<long, Forecast> DerWeather)
        {
            Dictionary<long, DerForecastDayAhead> Forecasts;
            Forecasts = derForcast;
            subDayAhead = await CalculateDayAheadSubstation(networkModel, DerWeather);
            CalculateSubstations(derForcast, Forecasts, subDayAhead).Wait();
            CalculateSubRegion(derForcast, networkModel).Wait();
            CalculateGeoRegions(derForcast, networkModel).Wait();
        }

        private async Task<Dictionary<long, DayAhead>> CalculateDayAheadSubstation(NetworkModelTransfer networkModel, Dictionary<long, Forecast> DerWeather)
        {
            //Cak
            CloudClient<ICache> cache = new CloudClient<ICache>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CECacheServiceListener"
            ); ;
            //

            List<EnergyConsumer> energyConsumers = new List<EnergyConsumer>();
            energyConsumers = GetEnergyConsumers(networkModel);
            Dictionary<long, DayAhead> calcDayAhead = new Dictionary<long, DayAhead>();//<-povratna
            ConsumerCharacteristics consumerCharacteristics = new ConsumerCharacteristics();

            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    DayAhead consumerDayAhead = consumerCharacteristics.GetDayAhead();

                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;

                        Forecast forecast = DerWeather[kvpDic.Key];//<-Kroz parametar

                        foreach (DERMSCommon.WeatherForecast.HourDataPoint dataPoint in consumerDayAhead.Hourly)
                        {
                            DarkSkyApi.Models.HourDataPoint hourDataPoint = forecast.Hourly.Hours.FirstOrDefault(h => h.Time.Hour == dataPoint.Time.Hour);
                            float curveFactor = 0;
                            curveFactor = dataPoint.ActivePower;
                            dataPoint.ActivePower = 0;
                            foreach (EnergyConsumer ec in energyConsumers)
                            {
                                if (gr.Equipments.Contains(ec.GlobalId))
                                {
                                    dataPoint.ActivePower += ec.PFixed * curveFactor;
                                }
                            }

                        }
                        calcDayAhead.Add(kvpDic.Key, consumerDayAhead.Clone());//<-povratna
                        await cache.InvokeWithRetryAsync(client => client.Channel.AddToSubstationDayAhead(kvpDic.Key, consumerDayAhead.Clone()));
                    }
                }

            }
            return calcDayAhead;
        }
        public async Task CalculateSubstations(Dictionary<long, DerForecastDayAhead> derForcast, Dictionary<long, DerForecastDayAhead> Forecasts, Dictionary<long, DayAhead> subDayAhead)
        {
            //Cak
            CloudClient<ICache> cache = new CloudClient<ICache>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CECacheServiceListener"
            ); ;
            //

            Dictionary<long, DerForecastDayAhead> substationForecast = Forecasts;
            foreach (KeyValuePair<long, DerForecastDayAhead> kvp in derForcast)
            {
                foreach (KeyValuePair<long, DayAhead> kvp2 in subDayAhead)
                {
                    if (kvp.Key.Equals(kvp2.Key))
                    {
                        kvp.Value.Consumption += subDayAhead[kvp.Key]; // Update na strani ceCache AddDerForecast
                        cache.InvokeWithRetryAsync(client => client.Channel.AddDerForecast(kvp.Value, kvp.Key,true)).Wait();
                    }
                }
            }
        }
        public async Task CalculateSubRegion(Dictionary<long, DerForecastDayAhead> derForcast, NetworkModelTransfer networkModel)
        {
            //Cak
            CloudClient<ICache> cache = new CloudClient<ICache>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CECacheServiceListener"
            ); ;
            //
            List<Substation> substations = new List<Substation>();
            substations = GetSubstations(networkModel);
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        derForcast[gr.GlobalId].Consumption = new DayAhead();
                        foreach (Substation substation in substations)
                        {
                            if (gr.Substations.Contains(substation.GlobalId))
                            {
                                derForcast[gr.GlobalId].Consumption += derForcast[substation.GlobalId].Consumption; // Update na strani ceCache
                                cache.InvokeWithRetryAsync(client => client.Channel.AddDerForecast(derForcast[gr.GlobalId], gr.GlobalId, true)).Wait();
                            }
                        }
                    }
                }
            }
        }
        public async Task CalculateGeoRegions(Dictionary<long, DerForecastDayAhead> derForcast, NetworkModelTransfer networkModel)
        {
            //Cak
            CloudClient<ICache> cache = new CloudClient<ICache>
            (
                serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "CECacheServiceListener"
            ); ;
            //
            List<SubGeographicalRegion> geographicalRegions = new List<SubGeographicalRegion>();
            geographicalRegions = GetSubGeographicalRegions(networkModel);
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("GeographicalRegion"))
                    {
                        var gr = (GeographicalRegion)kvpDic.Value;
                        derForcast[gr.GlobalId].Consumption = new DayAhead();
                        foreach (SubGeographicalRegion subGeoRegion in geographicalRegions)
                        {
                            if (gr.Regions.Contains(subGeoRegion.GlobalId))
                            {
                                derForcast[gr.GlobalId].Consumption += derForcast[subGeoRegion.GlobalId].Consumption; // Update na strani ceCache
                                cache.InvokeWithRetryAsync(client => client.Channel.AddDerForecast(derForcast[gr.GlobalId], gr.GlobalId, true)).Wait();
                            }
                        }
                    }
                }
            }
        }
        public List<EnergyConsumer> GetEnergyConsumers(NetworkModelTransfer networkModel)
        {
            List<EnergyConsumer> energyConsumers = new List<EnergyConsumer>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("EnergyConsumer"))
                    {
                        var gr = (EnergyConsumer)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }
        public List<Substation> GetSubstations(NetworkModelTransfer networkModel)
        {
            List<Substation> energyConsumers = new List<Substation>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("Substation"))
                    {
                        var gr = (Substation)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }
        public List<SubGeographicalRegion> GetSubGeographicalRegions(NetworkModelTransfer networkModel)
        {
            List<SubGeographicalRegion> energyConsumers = new List<SubGeographicalRegion>();
            foreach (KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> kvp in networkModel.Insert)
            {
                foreach (KeyValuePair<long, IdentifiedObject> kvpDic in kvp.Value)
                {
                    var type = kvpDic.Value.GetType();
                    if (type.Name.Equals("SubGeographicalRegion"))
                    {
                        var gr = (SubGeographicalRegion)kvpDic.Value;
                        energyConsumers.Add(gr);
                    }
                }
            }
            return energyConsumers;
        }
    }
}
