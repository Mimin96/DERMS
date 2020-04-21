using CalculationEngineServiceCommon;
using Common;
using dCom.Configuration;
using dCom.Simulation;
using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using Modbus.Acquisition;
using Modbus.Connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace dCom.ViewModel
{
    internal class MainViewModel : ViewModelBase, IDisposable, IStateUpdater
    {
        public ObservableCollection<BasePointItem> Points { get; set; }
        private ISendDataToCEThroughScada ProxyUI { get; set; }
        private ChannelFactory<ISendDataToCEThroughScada> factoryUI;
        private ServiceHost serviceHostForNMS;
        private ServiceHost serviceHostForCE;
        private ITransactionListing ProxyTM { get; set; }
        private ChannelFactory<ITransactionListing> factoryTM;

        #region Fields

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
        private IFunctionExecutor commandExecutor;
        private bool timerThreadStopSignal = true;
        private bool disposed = false;
        IConfiguration configuration;
        EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
        private WheaterSimulator ws = new WheaterSimulator();
        private int brojac = 0;
        #endregion Fields

        #region Properties

        public DateTime CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
                OnPropertyChanged("CurrentTime");
            }
        }

        public ConnectionState ConnectionState
        {
            get
            {
                return connectionState;
            }

            set
            {
                connectionState = value;
                OnPropertyChanged("ConnectionState");
            }
        }

        public string LogText
        {
            get
            {
                return logText;
            }

            set
            {
                logText = value;
                OnPropertyChanged("LogText");
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
                OnPropertyChanged("ElapsedTime");
            }
        }

        #endregion Properties

        public MainViewModel()
        {
            //Connect to TM
            NetTcpBinding binding4 = new NetTcpBinding();
            factoryTM = new ChannelFactory<ITransactionListing>(binding4, new EndpointAddress("net.tcp://localhost:20508/ITransactionListing"));
            ProxyTM = factoryTM.CreateChannel();

            Console.WriteLine("Connected: net.tcp://localhost:20508/ITransactionListing");
            ProxyTM.Enlist("net.tcp://localhost:19518/ITransactionCheck");

            //configuration = new ConfigReader();
            //commandExecutor = new FunctionExecutor(this, configuration);
            //this.acquisitor = new Acquisitor(acquisitionTrigger, this.commandExecutor, this, configuration);
            openConnection();
            //InitializePointCollection();
            InitializeAndStartThreads();
            logBuilder = new StringBuilder();
            ConnectionState = ConnectionState.CONNECTED;
            Thread.CurrentThread.Name = "Main Thread";



        }

        #region Private methods
        private void openConnection()
        {
            //Open service for NMS
            string address3 = String.Format("net.tcp://localhost:19012/ISendDataFromNMSToScada");
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForNMS = new ServiceHost(typeof(SendDataFromNmsToScada));

            serviceHostForNMS.AddServiceEndpoint(typeof(ISendDataFromNMSToScada), binding, address3);
            serviceHostForNMS.Open();
            Console.WriteLine("Open: net.tcp://localhost:19012/ISendDataFromNMSToScada");


            //Open service for NMS
            string address = String.Format("net.tcp://localhost:18503/ISendListOfGeneratorsToScada");
            NetTcpBinding binding2 = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForCE = new ServiceHost(typeof(SendListOfGeneratorsToScada));

            serviceHostForCE.AddServiceEndpoint(typeof(ISendListOfGeneratorsToScada), binding2, address);
            serviceHostForCE.Open();
            Console.WriteLine("Open: net.tcp://localhost:19012/ISendListOfGeneratorsToScada");
            //Open service for TM
            SendDataFromNmsToScada nmsToScada = new SendDataFromNmsToScada();
            string address4 = String.Format("net.tcp://localhost:19518/ITransactionCheck");
            NetTcpBinding binding4 = new NetTcpBinding();
            binding4.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            ServiceHost serviceHostForTM = new ServiceHost(new SCADATranscation(nmsToScada));
            var behaviour = serviceHostForTM.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            serviceHostForTM.AddServiceEndpoint(typeof(ITransactionCheck), binding4, address4);
            serviceHostForTM.Open();

            Console.WriteLine("Open: net.tcp://localhost:19518/ITransactionCheck");
        }
        private void InitializePointCollection()
        {
            List<DERMSCommon.SCADACommon.DataPoint> datapoints = new List<DERMSCommon.SCADACommon.DataPoint>();
            Points = new ObservableCollection<BasePointItem>();
            foreach (var c in configuration.GetConfigurationItems())
            {
                BasePointItem pi = CreatePoint(c, c.NumberOfRegisters, this.commandExecutor);
                if (pi != null)
                {
                    Points.Add(pi);
                    DERMSCommon.SCADACommon.PointType dad = (DERMSCommon.SCADACommon.PointType)pi.Type;
                    DERMSCommon.SCADACommon.DataPoint dataPoint = new DERMSCommon.SCADACommon.DataPoint((long)pi.Gid, (DERMSCommon.SCADACommon.PointType)pi.Type, pi.Address, pi.Timestamp, pi.Name, pi.DisplayValue, pi.RawValue, (DERMSCommon.SCADACommon.AlarmType)pi.Alarm, c.GidGeneratora);
                    datapoints.Add(dataPoint);
                }
            }

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            factoryUI = new ChannelFactory<ISendDataToCEThroughScada>(binding, new EndpointAddress("net.tcp://localhost:19999/ISendDataToCEThroughScada"));
            ProxyUI = factoryUI.CreateChannel();
            Console.WriteLine("Connected: net.tcp://localhost:19999/ISendDataToCEThroughScada");

            Dictionary<long, DERMSCommon.SCADACommon.CollectItem> collectItems = new Dictionary<long, DERMSCommon.SCADACommon.CollectItem>();
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();

            collectItems = ConvertDataPoints(datapoints);

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            string queryStmt1 = "INSERT INTO dbo.Collect(Timestamp, Gid, Production) VALUES(@Timestamp, @Gid, @Production)";
            //InsertInCollectTable(collectItems, queryStmt1, connectionString);

            //dayItems = ReadFromCollectTable(connectionString1);
            string queryStmt2 = "INSERT INTO dbo.Day(Gid, Pmin, Pmax, Pavg, E, Timestamp) VALUES(@Gid, @Pmin, @Pmax, @Pavg, @E, @Timestamp)";
            //InsertInDayTable(dayItems, queryStmt2, connectionString);

            ProxyUI.ReceiveFromScada(datapoints);
        }

        private double MinProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        {
            double minPerHour = double.MaxValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour) && d.Value.P < minPerHour)
                    minPerHour = d.Value.P;
            }

            return minPerHour;
        }

        private double MaxProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        {
            double maxPerHour = double.MinValue;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour) && d.Value.P > maxPerHour)
                    maxPerHour = d.Value.P;
            }

            return maxPerHour;
        }

        private double AvgProductionPerHour(int hour, int day, Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> dayItems)
        {
            int counter = 0;
            double sumPerHour = 0;
            foreach (var d in dayItems)
            {
                if (d.Key.Item2.DayOfYear.Equals(day) && d.Key.Item2.Hour.Equals(hour))
                {
                    counter++;
                    sumPerHour += d.Value.P;
                }
            }

            return sumPerHour / counter;
        }

        private Dictionary<long, DERMSCommon.SCADACommon.CollectItem> ConvertDataPoints(List<DERMSCommon.SCADACommon.DataPoint> datapoints)
        {
            Dictionary<long, DERMSCommon.SCADACommon.CollectItem> collectItems = new Dictionary<long, DERMSCommon.SCADACommon.CollectItem>();
            DERMSCommon.SCADACommon.CollectItem item = null;
            foreach (var dataPoint in datapoints)
            {
                item = new DERMSCommon.SCADACommon.CollectItem(dataPoint.Gid, 0, dataPoint.Timestamp);
                collectItems.Add(item.Gid, item);
            }

            return collectItems;
        }

        private Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> ReadFromCollectTable(string connectionString)
        {
            DERMSCommon.SCADACommon.DayItem itemDay = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem>();
            Tuple<long, DateTime> key = null;
            Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem> collectItemsData = new Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.CollectItem>();
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {

                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT Timestamp, Gid, Production FROM dbo.Collect", _con))
                {
                    _con.Open();
                    using (System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check is the reader has any rows at all before starting to read.
                        if (reader.HasRows)
                        {
                            // Read advances to the next row.
                            while (reader.Read())
                            {
                                DERMSCommon.SCADACommon.CollectItem c = new DERMSCommon.SCADACommon.CollectItem();
                                // To avoid unexpected bugs access columns by name.
                                try
                                {
                                    c.Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
                                    c.Gid = reader.GetInt64(reader.GetOrdinal("Gid"));
                                    c.P = reader.GetDouble(reader.GetOrdinal("Production"));
                                    key = new Tuple<long, DateTime>(c.Gid, c.Timestamp);

                                    collectItemsData.Add(key, c);
                                }
                                catch (Exception e)
                                { }
                            }
                        }
                    }

                    _con.Close();
                }
            }

            foreach (var d in collectItemsData)
            {
                itemDay = new DERMSCommon.SCADACommon.DayItem(d.Key.Item1, d.Key.Item2.Date.AddHours(d.Key.Item2.Hour), MinProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), MaxProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), AvgProductionPerHour(d.Key.Item2.Hour, d.Key.Item2.DayOfYear, collectItemsData), 0, 0);
                key = new Tuple<long, DateTime>(itemDay.Gid, itemDay.Timestamp);
                if (!dayItems.ContainsKey(key))
                    dayItems.Add(key, itemDay);
            }

            return dayItems;
        }

        private void InsertInDayTable(Dictionary<Tuple<long, DateTime>, DERMSCommon.SCADACommon.DayItem> dayItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                foreach (var day in dayItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Pmin", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param3 = _cmd.Parameters.Add("@Pmax", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param4 = _cmd.Parameters.Add("@Pavg", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param5 = _cmd.Parameters.Add("@E", System.Data.SqlDbType.Float);
                        System.Data.SqlClient.SqlParameter param6 = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);

                        param1.Value = day.Key.Item1;
                        param2.Value = day.Value.PMin;
                        param3.Value = day.Value.PMax;
                        param4.Value = day.Value.PAvg;
                        param5.Value = day.Value.E;
                        param6.Value = day.Value.Timestamp;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
            }
        }
        private void InsertInCollectTable(Dictionary<long, DERMSCommon.SCADACommon.CollectItem> collectItems, string query, string connectionString)
        {
            using (System.Data.SqlClient.SqlConnection _con = new System.Data.SqlClient.SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SCADA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
            {

                foreach (var c in collectItems)
                {
                    using (System.Data.SqlClient.SqlCommand _cmd = new System.Data.SqlClient.SqlCommand(query, _con))
                    {

                        System.Data.SqlClient.SqlParameter param = _cmd.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime);
                        System.Data.SqlClient.SqlParameter param1 = _cmd.Parameters.Add("@Gid", System.Data.SqlDbType.BigInt);
                        System.Data.SqlClient.SqlParameter param2 = _cmd.Parameters.Add("@Production", System.Data.SqlDbType.Float);

                        param.Value = c.Value.Timestamp;
                        param1.Value = c.Value.Gid;
                        param2.Value = c.Value.P;
                        _con.Open();
                        try
                        {
                            _cmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        { }
                        _con.Close();
                    }
                }
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

                brojac++;
                CurrentTime = DateTime.Now;
                ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 1));
                acquisitionTrigger.Set();
                if (brojac % 100 == 0)
                    ws.SimulateWheater();
                Thread.Sleep(1000);
            }
        }

        #endregion Private methods

        #region IStateUpdater implementation

        public void UpdateConnectionState(ConnectionState currentConnectionState)
        {
            dispather.Invoke((Action)(() =>
            {
                ConnectionState = currentConnectionState;
            }));
        }

        public void LogMessage(string message)
        {
            if (disposed)
                return;

            string threadName = Thread.CurrentThread.Name;

            dispather.Invoke((Action)(() =>
            {
                lock (lockObject)
                {
                    logBuilder.Append($"{DateTime.Now} [{threadName}]: {message}{Environment.NewLine}");
                    LogText = logBuilder.ToString();
                }
            }));
        }

        #endregion IStateUpdater implementation

        public void Dispose()
        {
            disposed = true;
            timerThreadStopSignal = false;
            (commandExecutor as IDisposable).Dispose();
            this.acquisitor.Dispose();
            acquisitionTrigger.Dispose();
        }
    }
}