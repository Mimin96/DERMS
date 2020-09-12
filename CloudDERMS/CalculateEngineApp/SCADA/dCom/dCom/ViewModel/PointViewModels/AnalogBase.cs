using CalculationEngineServiceCommon;
using Common;
using DERMSCommon.SCADACommon;
using DERMSCommon.TransactionManager;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Modbus;
using Modbus.FunctionParameters;
using ProcessingModule;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace dCom.ViewModel
{
    internal class AnalogBase : BasePointItem
    {
        private double eguValue;
        private ISendDataToCEThroughScada ProxyUI { get; set; }
        private ChannelFactory<ISendDataToCEThroughScada> factoryUI;
        private int brojac;
        private List<DataPoint> datapoints = new List<DataPoint>();
        //private ScadaDB scadaDB = new ScadaDB();
        private CloudClient<ISendDataToCEThroughScada> transactionCoordinator;

        public AnalogBase(Common.IConfigItem c, Common.IFunctionExecutor commandExecutor, Common.IStateUpdater stateUpdater, Common.IConfiguration configuration, int i)
            : base(c, commandExecutor, stateUpdater, configuration, i)
        {
            ProcessRawValue(RawValue);
            brojac = 0;
        }

        protected override async void CommandExecutor_UpdatePointEvent(Common.PointType type, ushort pointAddres, ushort newValue)
        {
            Thread.Sleep(100);

            if (this.type == type && this.address == pointAddres)
            {

                //OVDE UPISATI U BAZU - POPUNJAVAA SE DATA POINT SA PODACIMA

                RawValue = newValue;
                ProcessRawValue(newValue);
                Timestamp = DateTime.Now;
                DERMSCommon.SCADACommon.PointType dad = (DERMSCommon.SCADACommon.PointType)configItem.RegistryType;
                //NetTcpBinding binding = new NetTcpBinding();
                //binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
                //factoryUI = new ChannelFactory<ISendDataToCEThroughScada>(binding, new EndpointAddress("net.tcp://localhost:19999/ISendDataToCEThroughScada"));
                //ProxyUI = factoryUI.CreateChannel();
                //Console.WriteLine("Connected: net.tcp://localhost:19999/ISendDataToCEThroughScada");

                DataPoint dataPoint = new DataPoint((long)configItem.Gid, (DERMSCommon.SCADACommon.PointType)configItem.RegistryType, pointAddres, Timestamp, configItem.Description, DisplayValue, RawValue, (DERMSCommon.SCADACommon.AlarmType)alarm, configItem.GidGeneratora);

                datapoints.Add(dataPoint);

                transactionCoordinator = new CloudClient<ISendDataToCEThroughScada>
                (
                    serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                    partitionKey: new ServicePartitionKey(0),
                    clientBinding: WcfUtility.CreateTcpClientBinding(),
                    listenerName: "SendDataToCEThroughScadaListener"
                  );
                transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.ReceiveFromScada(datapoints)).Wait();

                // ProxyUI.ReceiveFromScada(datapoints);


                ComunicationSCADAClient sCADAClient = new ComunicationSCADAClient("SCADAEndpoint");
                await sCADAClient.SetDatabaseData(datapoints);
            }

        }

        public double EguValue
        {
            get
            {
                return eguValue;
            }

            set
            {
                eguValue = value;
                OnPropertyChanged("DisplayValue");
            }
        }

        private void ProcessRawValue(ushort newValue)
        {
            // TODO implement samo otkomentarisati
            EguValue = EGUConverter.ConvertToEGU(configItem.ScaleFactor, configItem.Deviation, newValue);
            ProcessAlarm(newValue);
        }

        private void ProcessAlarm(double eguValue)
        {
            alarm = AlarmProcessor.GetAlarmForAnalogPoint(eguValue, configItem);
            if (alarm != Common.AlarmType.NO_ALARM)
            {
                // string message = $" ima alarm";
                // this.stateUpdater.LogMessage(message);
            }
            OnPropertyChanged("Alarm");
        }

        public override string DisplayValue
        {
            get
            {
                return EguValue.ToString();
            }
        }

        public override string DisplayValueGid
        {
            get
            {
                return configItem.Gid.ToString();
            }
        }

        protected override void WriteCommand_Execute(object obj)
        {
            try
            {
                // TODO implement
                ushort raw = 0;
                raw = EGUConverter.ConvertToRaw(configItem.ScaleFactor, configItem.Deviation, CommandedValue);
                ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)GetWriteFunctionCode(type), address, raw, configuration);
                Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                this.commandExecutor.EnqueueCommand(fn);
            }
            catch (Exception ex)
            {
                string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
                this.stateUpdater.LogMessage(message);
            }
        }
    }
}
