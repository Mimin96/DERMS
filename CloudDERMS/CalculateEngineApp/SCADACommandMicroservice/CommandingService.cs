using CalculationEngineServiceCommon;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.SCADACommon;
using DERMSCommon.TransactionManager;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommandMicroservice
{
    public class CommandingService : ISendListOfGeneratorsToScada
    {

        public CommandingService()
        {

        }

        public void SendListOfGenerators(Dictionary<long, double> generators)
        {

            CloudClient<IScadaCloudToScadaLocal> transactionCoordinator = new CloudClient<IScadaCloudToScadaLocal>
            (
                serviceUri: new Uri($"fabric:/SCADAApp/SCADACacheMicroservice"),
                partitionKey: new ServicePartitionKey(0),
                clientBinding: WcfUtility.CreateTcpClientBinding(),
                listenerName: "SCADAComunicationMicroserviceListener"
            );

            Dictionary<long, IdentifiedObject> analogniStari = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetAnalogniKontejnerModel()).Result;
            Dictionary<long, IdentifiedObject> digitalniStari = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetDigitalniKontejnerModel()).Result;
            Dictionary<List<long>, ushort> GidoviNaAdresu = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetGidoviNaAdresuModel()).Result;

            string clientAddress = transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.GetAddress()).Result;
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<IScadaCloudServer>(binding, new EndpointAddress(clientAddress));
            IScadaCloudServer Proxy = factory.CreateChannel();

            foreach (KeyValuePair<long, double> generator in generators)
            {
                foreach (KeyValuePair<List<long>, ushort> gidoviNaAdresu in GidoviNaAdresu)
                {
                    if (generator.Key == gidoviNaAdresu.Key[0])
                    {
                        if (analogniStari.Keys.Contains(gidoviNaAdresu.Key[1]))
                        {
                            if (analogniStari[gidoviNaAdresu.Key[1]].Description == "Commanding")
                            {
                                {

                                    KeyValuePair<long, IdentifiedObject> a = analogniStari.ElementAt(gidoviNaAdresu.Value - 3000 - 1);
                                    float zbir = ((Analog)a.Value).NormalValue + (float)generator.Value * ((Analog)a.Value).NormalValue / 100;
                                    ((Analog)a.Value).NormalValue = zbir;
                                    zbir = (float)Math.Round(zbir);
                                    double vred = (generator.Value * ((Analog)a.Value).NormalValue / 100);
                                    vred = (double)Math.Round(vred);
                                    if (vred < 0)
                                    {
                                        vred = vred * (-1);
                                    }

                                    Proxy.SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, gidoviNaAdresu.Value, (ushort)vred);

                                    Proxy.SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(gidoviNaAdresu.Value - 2), (ushort)zbir);

                                }
                            }
                        }
                        else if (digitalniStari.Keys.Contains(gidoviNaAdresu.Key[1]))
                        {
                            if (digitalniStari[gidoviNaAdresu.Key[1]].Description == "Commanding")
                            {
                                {

                                    Proxy.SendCommandToSimlator(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, gidoviNaAdresu.Value, (ushort)generator.Value);

                                }
                            }
                        }
                    }

                }

            }

            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddorUpdateAnalogniKontejnerModelEntity(analogniStari));
            transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.AddorUpdateDigitalniKontejnerModelEntity(digitalniStari));

        }
    }
}
