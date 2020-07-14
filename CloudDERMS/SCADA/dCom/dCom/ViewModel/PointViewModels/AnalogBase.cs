using CalculationEngineServiceCommon;
using Common;
using DERMSCommon.SCADACommon;
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
        private int brojac = 0;
        private List<DataPoint> datapoints = new List<DataPoint>();
        private ScadaDB scadaDB = new ScadaDB();

        public AnalogBase(Common.IConfigItem c, Common.IFunctionExecutor commandExecutor, Common.IStateUpdater stateUpdater, Common.IConfiguration configuration, int i)
            : base(c, commandExecutor, stateUpdater, configuration, i)
        {
            ProcessRawValue(RawValue);
        }

        protected override void CommandExecutor_UpdatePointEvent(Common.PointType type, ushort pointAddres, ushort newValue)
        {
            Thread.Sleep(100);

            if (this.type == type && this.address == pointAddres)
            {

                //OVDE UPISATI U BAZU - POPUNJAVAA SE DATA POINT SA PODACIMA

                RawValue = newValue;
                ProcessRawValue(newValue);
                Timestamp = DateTime.Now;
                DERMSCommon.SCADACommon.PointType dad = (DERMSCommon.SCADACommon.PointType)configItem.RegistryType;
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
                factoryUI = new ChannelFactory<ISendDataToCEThroughScada>(binding, new EndpointAddress("net.tcp://localhost:19999/ISendDataToCEThroughScada"));
                ProxyUI = factoryUI.CreateChannel();
                Console.WriteLine("Connected: net.tcp://localhost:19999/ISendDataToCEThroughScada");

                DataPoint dataPoint = new DataPoint((long)configItem.Gid, (DERMSCommon.SCADACommon.PointType)configItem.RegistryType, pointAddres, Timestamp, configItem.Description, DisplayValue, RawValue, (DERMSCommon.SCADACommon.AlarmType)alarm, configItem.GidGeneratora);

                datapoints.Add(dataPoint);
                ProxyUI.ReceiveFromScada(datapoints);

                Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem>();
                Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();
                Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem>();
                Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem> yearItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem>();

                collectItems = scadaDB.ConvertDataPoints(datapoints);

                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                string queryStmt1 = "INSERT INTO dbo.Collect(Timestamp, Gid, Production) VALUES(@Timestamp, @Gid, @Production)";
                scadaDB.InsertInCollectTable(collectItems, queryStmt1, connectionString);

                dayItems = scadaDB.ReadFromCollectTable(connectionString);
                string queryStmt2 = "INSERT INTO dbo.Day(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
                scadaDB.InsertInDayTable(dayItems, queryStmt2, connectionString);

                string queryStmt3 = "INSERT INTO dbo.Month(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
                monthItems = scadaDB.ReadFromDayTable(connectionString);
                scadaDB.InsertInMonthTable(monthItems, queryStmt3, connectionString);

                string queryStmt4 = "INSERT INTO dbo.Year(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
                yearItems = scadaDB.ReadFromMonthTable(connectionString);
                scadaDB.InsertInYearTable(yearItems, queryStmt4, connectionString);
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
