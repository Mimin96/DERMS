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
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dCom.ViewModel
{
    internal class DigitalBase : BasePointItem
    {
        private Common.DState state;
        private ISendDataToCEThroughScada ProxyUI { get; set; }
        private ChannelFactory<ISendDataToCEThroughScada> factoryUI;
        private CloudClient<ISendDataToCEThroughScada> transactionCoordinator;

        public DigitalBase(Common.IConfigItem c, Common.IFunctionExecutor commandExecutor, Common.IStateUpdater stateUpdater, Common.IConfiguration configuration, int i)
            : base(c, commandExecutor, stateUpdater, configuration, i)
        {

            ProcessRawValue(RawValue);
        }

        public Common.DState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("DisplayValue");
            }
        }

        public override string DisplayValue
        {
            get
            {
                return State.ToString();
            }
        }
        public override string DisplayValueGid
        {
            get
            {
                return configItem.Gid.ToString();
            }
        }
        protected override async void CommandExecutor_UpdatePointEvent(Common.PointType type, ushort pointAddres, ushort newValue)
        {
            Thread.Sleep(100);

            if (this.type == type && this.address == pointAddres && newValue != RawValue)
            {
                //OVDE UPISATI U BAZU - POPUNJAVAA SE DATA POINT SA PODACIMA

                RawValue = newValue;
                ProcessRawValue(newValue);
                Timestamp = DateTime.Now;
                DERMSCommon.SCADACommon.PointType dad = (DERMSCommon.SCADACommon.PointType)configItem.RegistryType;
                DataPoint dataPoint = new DataPoint((long)configItem.Gid, (DERMSCommon.SCADACommon.PointType)configItem.RegistryType, pointAddres, Timestamp, configItem.Description, DisplayValue, RawValue, (DERMSCommon.SCADACommon.AlarmType)alarm, configItem.GidGeneratora);
                List<DataPoint> datapoints = new List<DataPoint>();
                datapoints.Add(dataPoint);

                transactionCoordinator = new CloudClient<ISendDataToCEThroughScada>
                (
                    serviceUri: new Uri("fabric:/CalculateEngineApp/CECacheMicroservice"),
                    partitionKey: new ServicePartitionKey(0),
                    clientBinding: WcfUtility.CreateTcpClientBinding(),
                    listenerName: "SendDataToCEThroughScadaListener"
                  );
                await transactionCoordinator.InvokeWithRetryAsync(client => client.Channel.ReceiveFromScada(datapoints));



            }
        }

        private void ProcessRawValue(ushort newValue)
        {
            State = (Common.DState)newValue;
            // TODO implement samo otkomentarisati
            ProcessAlarm(newValue);
        }

        private void ProcessAlarm(ushort state)
        {
            alarm = AlarmProcessor.GetAlarmForDigitalPoint(RawValue, configItem);
            OnPropertyChanged("Alarm");
        }

        protected override void WriteCommand_Execute(object obj)
        {
            try
            {
                ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)GetWriteFunctionCode(type), address, (ushort)CommandedValue, configuration);
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
