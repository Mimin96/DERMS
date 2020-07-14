using CalculationEngineServiceCommon;
using Common;
using dCom.Simulation;
using dCom.ViewModel;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.NMSCommuication;
using Modbus;
using Modbus.Acquisition;
using Modbus.Connection;
using Modbus.FunctionParameters;
using ProcessingModule;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace dCom.Configuration
{
    public class SendDataFromNmsToScada : SCADACommunication, ISendDataFromNMSToScada, IStateUpdater
    {

        private static SignalsTransfer signalsTransfer;
        public static SignalsTransfer SignalsTransfer { get => signalsTransfer; set => signalsTransfer = value; }
        private ObservableCollection<BasePointItem> PointsToAdd { get; set; }
        private ISendDataToCEThroughScada ProxyUI { get; set; }
        private ChannelFactory<ISendDataToCEThroughScada> factoryUI;


        private object lockObject = new object();
        private Thread timerWorker;
        private ConnectionState connectionState;

        private Modbus.Acquisition.Acquisitor acquisitor;
        private AutoResetEvent acquisitionTrigger = new AutoResetEvent(false);
        private TimeSpan elapsedTime = new TimeSpan();
        private Dispatcher dispather = Dispatcher.CurrentDispatcher;
        private string logText;
        private StringBuilder logBuilder;
        private DateTime currentTime;

        private bool timerThreadStopSignal = true;
        private bool disposed = false;

        //private DERMSCommon.SCADACommon.ScadaDB scadaDB = new DERMSCommon.SCADACommon.ScadaDB();

        public ConnectionState ConnectionState
        {
            get
            {
                return connectionState;
            }

            set
            {
                connectionState = value;

            }
        }
        public TimeSpan ElapsedTime
        {
            get
            {
                return elapsedTime;
            }

            set
            {
                elapsedTime = value;
                //OnPropertyChanged("ElapsedTime");
            }
        }
        public DateTime CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
                //OnPropertyChanged("CurrentTime");
            }
        }
        public bool CheckForTM(SignalsTransfer signals)
        {
            SignalsTransfer = signals;
            if (signals != null)
                return true;
            else
                return false;
        }
        public bool SendGids(SignalsTransfer signals)
        {
            Dictionary<long, IdentifiedObject> analogni = new Dictionary<long, IdentifiedObject>();
            Dictionary<long, IdentifiedObject> digitalni = new Dictionary<long, IdentifiedObject>();

            if (SignalsTransfer.Insert.Count > 0 || SignalsTransfer.Update.Count > 0)
            {

                analogni = SignalsTransfer.Insert[1];
                digitalni = SignalsTransfer.Insert[0];
                foreach (KeyValuePair<long, IdentifiedObject> kvp in analogni)
                {
                    if (!analogniStari.ContainsKey(kvp.Key))
                    {
                        analogniStari.Add(kvp.Key, kvp.Value);

                    }
                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    if (!digitalniStari.ContainsKey(kvp.Key))
                    {
                        digitalniStari.Add(kvp.Key, kvp.Value);

                    }
                }

                configuration = new ConfigReader(analogniStari, digitalniStari);
                commandExecutor = new FunctionExecutor(this, configuration);
                foreach (KeyValuePair<long, IdentifiedObject> kvp in analogni)
                {

                    foreach (KeyValuePair<List<long>, ushort> par in GidoviNaAdresu)
                    {
                        if (par.Key.Contains(((Analog)kvp.Value).GlobalId))
                        {
                            ushort raw = 0;
                            if ((double)((Analog)kvp.Value).NormalValue != 0)
                            {
                                raw = (ushort)((Analog)kvp.Value).NormalValue;
                            }

                            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, par.Value, raw, configuration);
                            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                            commandExecutor.EnqueueCommand(fn);
                        }
                    }

                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    foreach (KeyValuePair<List<long>, ushort> par in GidoviNaAdresu)
                    {
                        if (par.Key.Contains(((Discrete)kvp.Value).GlobalId))
                        {

                            ModbusWriteCommandParameters p = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, par.Value, (ushort)((Discrete)kvp.Value).NormalValue, configuration);
                            Common.IModbusFunction fn = FunctionFactory.CreateModbusFunction(p);
                            commandExecutor.EnqueueCommand(fn);
                        }
                    }

                }



                
                //Thread.Sleep(10000);
                this.acquisitor = new Acquisitor(acquisitionTrigger, commandExecutor, this, configuration);

                InitializePointCollection();
                commandExecutor.UpdatePointEvent += CommandExecutor_UpdatePointEvent;
                InitializeAndStartThreads();


            }
            signals = SignalsTransfer;
            if (signals != null)
                return true;
            else
                return false;
        }


        /// <summary>
        /// Method for handling received points.
        /// </summary>
        /// <param name="type">The point type.</param>
        /// <param name="pointAddress">The point address.</param>
        /// <param name="newValue">The new value.</param>
        private void CommandExecutor_UpdatePointEvent(PointType type, ushort pointAddress, ushort newValue)
        {

        }


        private void InitializePointCollection()
        {
            List<DERMSCommon.SCADACommon.DataPoint> datapoints = new List<DERMSCommon.SCADACommon.DataPoint>();
            PointsToAdd = new ObservableCollection<BasePointItem>();
            foreach (var c in configuration.GetConfigurationItems())
            {

                BasePointItem pi = CreatePoint(c, c.NumberOfRegisters, commandExecutor);
                if (pi != null)

                    PointsToAdd.Add(pi);
                DERMSCommon.SCADACommon.PointType dad = (DERMSCommon.SCADACommon.PointType)pi.Type;
                DERMSCommon.SCADACommon.DataPoint dataPoint = new DERMSCommon.SCADACommon.DataPoint((long)pi.Gid, (DERMSCommon.SCADACommon.PointType)pi.Type, pi.Address, pi.Timestamp, pi.Name, pi.DisplayValue, pi.RawValue, (DERMSCommon.SCADACommon.AlarmType)pi.Alarm, c.GidGeneratora);
                datapoints.Add(dataPoint);
            }

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factoryUI = new ChannelFactory<ISendDataToCEThroughScada>(binding, new EndpointAddress("net.tcp://localhost:19999/ISendDataToCEThroughScada"));
            ProxyUI = factoryUI.CreateChannel();
            Console.WriteLine("Connected: net.tcp://localhost:19999/ISendDataToCEThroughScada");


            ProxyUI.ReceiveFromScada(datapoints);

            //Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem>();
            //Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();
            //Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem> monthItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.MonthItem>();
            //Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem> yearItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.YearItem>();


            //collectItems = scadaDB.ConvertDataPoints(datapoints);

            //string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            //string queryStmt1 = "INSERT INTO dbo.Collect(Timestamp, Gid, Production) VALUES(@Timestamp, @Gid, @Production)";
            //scadaDB.InsertInCollectTable(collectItems, queryStmt1, connectionString);

            //Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            //using (StreamReader reader = new StreamReader("C:/Users/Nemanja/Desktop/collectData.txt"))
            //{
            //    while (reader.ReadLine() != null)
            //    {
            //        keyValuePairs.Add(reader.ReadLine().Split('|')[0] + "|" + reader.ReadLine().Split('|')[1], reader.ReadLine().Split('|')[0] + "|" + reader.ReadLine().Split('|')[1] + "|" + reader.ReadLine().Split('|')[2]);
            //    }
            //}

            //using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            //{
            //    foreach (var d in keyValuePairs)
            //    {
            //        using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(queryStmt1, _con))
            //        {

            //            System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
            //            System.Data.SqlClient.SqlParameter param6 = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
            //            System.Data.SqlClient.SqlParameter param7 = _cmd.Parameters.Add("@P", System.Data.SqlDbType.Float);

            //            param1.Value = d.Value.Split('|')[0];
            //            param6.Value = d.Value.Split('|')[1];
            //            param7.Value = d.Value.Split('|')[2];
            //            _con.Open();
            //            try
            //            {
            //                _cmd.ExecuteNonQuery();
            //            }
            //            catch (Exception e)
            //            { }

            //            _con.Close();
            //        }
            //    }
            //}

            //dayItems = scadaDB.ReadFromCollectTable(connectionString);
            //string queryStmt2 = "INSERT INTO dbo.Day(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
            //scadaDB.InsertInDayTable(dayItems, queryStmt2, connectionString);

            //string queryStmt3 = "INSERT INTO dbo.Month(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
            //monthItems = scadaDB.ReadFromDayTable(connectionString);
            //scadaDB.InsertInMonthTable(monthItems, queryStmt3, connectionString);

            //string queryStmt4 = "INSERT INTO dbo.Year(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
            //yearItems = scadaDB.ReadFromMonthTable(connectionString);
            //scadaDB.InsertInYearTable(yearItems, queryStmt4, connectionString);

        }
        private void InitializeAndStartThreads()
        {
            InitializeTimerThread();
            StartTimerThread();
        }

        private void InitializeTimerThread()
        {
            timerWorker = new Thread(TimerWorker_DoWork);
            timerWorker.Name = "Timer Thread";
        }

        private void StartTimerThread()
        {
            timerWorker.Start();
        }

        /// <summary>
        /// Timer thread:
        ///		Refreshes timers on UI and signalizes to acquisition thread that one second has elapsed
        /// </summary>
        private void TimerWorker_DoWork()
        {
            while (timerThreadStopSignal)
            {
                if (disposed)
                    return;

                CurrentTime = DateTime.Now;
                ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 1));
                // acquisitionTrigger.Set();
                //ModbusWriteCommandParameters p1 = new ModbusWriteCommandParameters(6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, 0, 0, configuration);
                //Common.IModbusFunction fn1 = FunctionFactory.CreateModbusFunction(p1);
                //commandExecutor.EnqueueCommand(fn1);
                //Thread.Sleep(6000);
            }
        }



        private BasePointItem CreatePoint(IConfigItem c, int i, IFunctionExecutor commandExecutor)
        {
            switch (c.RegistryType)
            {
                case PointType.DIGITAL_INPUT:
                    return new DigitalInput(c, commandExecutor, this, configuration, i);

                case PointType.DIGITAL_OUTPUT:
                    return new DigitalOutput(c, commandExecutor, this, configuration, i);

                case PointType.ANALOG_INPUT:
                    return new AnalaogInput(c, commandExecutor, this, configuration, i);

                case PointType.ANALOG_OUTPUT:
                    return new AnalogOutput(c, commandExecutor, this, configuration, i);

                default:
                    return null;
            }
        }

        public void UpdateConnectionState(ConnectionState currentConnectionState)
        {
            dispather.Invoke((Action)(() =>
            {
                ConnectionState = currentConnectionState;
            }));
        }

        public void LogMessage(string message)
        {

        }
    }
}

