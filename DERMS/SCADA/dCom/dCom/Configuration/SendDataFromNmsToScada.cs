using CalculationEngineServiceCommon;
using Common;
using dCom.Simulation;
using dCom.ViewModel;
using DERMSCommon.DataModel.Core;
using DERMSCommon.NMSCommuication;
using Modbus.Acquisition;
using Modbus.Connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                        analogniStari.Add(kvp.Key, kvp.Value);
                }
                foreach (KeyValuePair<long, IdentifiedObject> kvp in digitalni)
                {
                    if (!digitalniStari.ContainsKey(kvp.Key))
                        digitalniStari.Add(kvp.Key, kvp.Value);
                }



                configuration = new ConfigReader(analogniStari, digitalniStari);
                commandExecutor = new FunctionExecutor(this, configuration);
                this.acquisitor = new Acquisitor(acquisitionTrigger, commandExecutor, this, configuration);

                InitializePointCollection();
                InitializeAndStartThreads();

                commandExecutor.UpdatePointEvent += CommandExecutor_UpdatePointEvent;
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
                acquisitionTrigger.Set();
                Thread.Sleep(1000);
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

