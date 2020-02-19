using DermsUI.Model;
using DermsUI.Model.ThreeViewModel;
using DermsUI.Resources;
using DermsUI.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using DERMSCommon;
using DermsUI.MediatorPattern;
using DermsUI.Communication;
using DERMSCommon.SCADACommon;
using DERMSCommon.NMSCommuication;
using DERMSCommon.DataModel.Core;
using GMap.NET;

namespace DermsUI.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private List<DataPoint> Points;
        private CommunicationProxy proxy;
        private ClientSideProxy ClientSideProxy { get; set; }
        private CalculationEnginePubSub CalculationEnginePubSub { get; set; }

        #region Model Management
        private UserControl _content;
        private bool _isMenuOpen;
        private string _criteria;
        private IEnumerable<SampleGroupVm> _samples;
        private readonly IEnumerable<SampleGroupVm> _dataSource;
        #endregion
        #region Commanding
        private UserControl _content2;
        private bool _isMenuOpen2;
        private string _criteria2;
        private IEnumerable<SampleGroupVm> _samples2;
        private readonly IEnumerable<SampleGroupVm> _dataSource2;
        #endregion
        #region Monitoring
        private UserControl _content3;
        private bool _isMenuOpen3;
        private string _criteria3;
        private IEnumerable<SampleGroupVm> _samples3;
        private readonly IEnumerable<SampleGroupVm> _dataSource3;
        #endregion
        #region Loggs
        private UserControl _content4;
        private bool _isMenuOpen4;
        private string _criteria4;
        private IEnumerable<SampleGroupVm> _samples4;
        private readonly IEnumerable<SampleGroupVm> _dataSource4;
        #endregion
        #region TreeView
        private List<EnergyNetwork> energyNetworks;
        public MyICommand<long> GeographicalRegionCommand { get; private set; }
        public MyICommand<long> EnergyNetworkCommand { get; private set; }
        public MyICommand<long> SubstationCommand { get; private set; }
        public MyICommand<long> FeederCommand { get; private set; }
        #endregion

        public HomeViewModel()
        {
            Mediator.Register("GetAlarmSignals", GetAlarmSignals);
            Mediator.Register("GetAllSignals", GetAllSignals);
            Mediator.Register("SCADAData", GetSignalsFromProxy);
            Mediator.Register("NMSNetworkModelData", GetNetworkModelFromProxy);
            Mediator.Register("SCADACommanding", SCADACommanding);

            ClientSideProxy = new ClientSideProxy();
            CalculationEnginePubSub = new CalculationEnginePubSub();
            ClientSideProxy.StartServiceHost(CalculationEnginePubSub);
            ClientSideProxy.Subscribe(1);

            //proxy = new CommunicationProxy();
            //proxy.Open();

            Points = new List<DataPoint>();

            #region TreeView
            EnergyNetworks = new List<EnergyNetwork>() { new EnergyNetwork() };
            EnergyNetworkCommand = new MyICommand<long>(myEnergyNetworkCommand);
            GeographicalRegionCommand = new MyICommand<long>(myGeographicalRegionCommand);
            SubstationCommand = new MyICommand<long>(mySubstationCommand);
            FeederCommand = new MyICommand<long>(myFeederCommand);
            #endregion

            #region Model Management
            IsMenuOpen = true;
            _dataSource = new[]
            {
                new SampleGroupVm
                {
                    Name = "Model Management",
                    Items = new[]
                    {
                        new SampleVm("CIM Profile Creator", typeof(View.CimProfileCreator)),
                        new SampleVm("Create/Apply Delta", typeof(View.CreateApplyDelta))
                    }
                },
                new SampleGroupVm
                {
                    Name = "Create new entites",
                    Items = new []
                    {
                        new SampleVm("Terminal",typeof(View.AddNewTerminal)),
                        new SampleVm("Connectivity Node",typeof(View.AddNewConnectivityNode)),
                        new SampleVm("Energy Consumer",typeof(View.AddNewEnergyConsumer)),
                        new SampleVm("Synchronous Machine",typeof(View.AddNewGenerator)),
                        new SampleVm("Breaker",typeof(View.AddNewBreaker)),
                        new SampleVm("Geographical Region",typeof(View.AddNewGeoRegion)),
                        new SampleVm("Analog Signal",typeof(View.AddNewAnalogSignal)),
                        new SampleVm("Digital Signal",typeof(View.AddNewDigitalSignal)),
                        new SampleVm("Feeder",typeof(View.AddNewFeederObject)),
                        new SampleVm("Substation",typeof(View.AddNewSubstation)),
                    }
                }
            };

            _samples = _dataSource;
            #endregion
            #region Scada
            IsMenuOpen2 = true;
            _dataSource2 = new[]
            {
                new SampleGroupVm
                {
                    Name = "Data",
                    Items = new []
                    {
                        new SampleVm("SCADA Data",typeof(View.SCADAView)),
                        new SampleVm("SCADA Alarms",typeof(View.Alarms)),
                    }
                }
            };

            _samples2 = _dataSource2;
            #endregion
            #region Monitoring
            IsMenuOpen3 = true;
            _dataSource3 = new[]
            {
                new SampleGroupVm
                {
                    Name = "Create new entites",
                    Items = new []
                    {
                        new SampleVm("Terminal",typeof(View.AddNewTerminal)),
                    }
                }
            };

            _samples3 = _dataSource3;
            #endregion
            #region Loggs
            IsMenuOpen4 = true;
            _dataSource4 = new[]
            {
                new SampleGroupVm
                {
                    Name = "Loggs",
                    Items = new[]
                    {
                        new SampleVm("SCADA Loggs", typeof(View.SCADALoggs)),
                        new SampleVm("Calculate Engine Loggs", typeof(View.CELoggs)),
                        new SampleVm("NMS Loggs", typeof(View.NMSLoggs)),
                        new SampleVm("Transaction Manager Loggs", typeof(View.TMLoggs)),
                        new SampleVm("UI Loggs", typeof(View.UILoggs)),
                    }
                },
            };

            _samples4 = _dataSource4;
            #endregion

            Logger.Log("UI is started.", DERMSCommon.Enums.Component.UI, DERMSCommon.Enums.LogLevel.Info);
        }

        #region Mediator
        private void SCADACommanding(object parameter) 
        {
            DERMSCommon.SCADACommon.SCADACommanding commanding = (DERMSCommon.SCADACommon.SCADACommanding)parameter;

            proxy.sendToCE.UpdateThroughUI(commanding);
        }

        private void GetNetworkModelFromProxy(object parameter) 
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
        }

        public void GetNetworkModelGeneratorFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            Dictionary<long, IdentifiedObject> ioGenerator = networkModel.Insert[FTN.Common.DMSType.GENERATOR];
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za generatore

            foreach (var item in ioGenerator.Keys)
            {
                DERMSCommon.DataModel.Core.Generator g = ioGenerator[item] as DERMSCommon.DataModel.Core.Generator;
                coordinates.Add(new PointLatLng( g.Latitude, g.Longitude));
            }
        }

        public void GetNetworkModelSourceFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za EnergySource
            Dictionary<long, IdentifiedObject> ioSource = networkModel.Insert[FTN.Common.DMSType.ENEGRYSOURCE];

            foreach (var item in ioSource.Keys)
            {
                DERMSCommon.DataModel.Core.EnergySource s = ioSource[item] as DERMSCommon.DataModel.Core.EnergySource;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelioConsumerFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za EnergyConsumer
            Dictionary<long, IdentifiedObject> ioConsumer = networkModel.Insert[FTN.Common.DMSType.ENERGYCONSUMER];

            foreach (var item in ioConsumer.Keys)
            {
                DERMSCommon.DataModel.Core.EnergyConsumer s = ioConsumer[item] as DERMSCommon.DataModel.Core.EnergyConsumer;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelioGeographicalRegionFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za GeographicalRegion
            Dictionary<long, IdentifiedObject> ioGRegion = networkModel.Insert[FTN.Common.DMSType.GEOGRAPHICALREGION];

            foreach (var item in ioGRegion.Keys)
            {
                DERMSCommon.DataModel.Core.GeographicalRegion s = ioGRegion[item] as DERMSCommon.DataModel.Core.GeographicalRegion;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelioSubGeographicalRegionFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za SubGeographicalRegion
            Dictionary<long, IdentifiedObject> ioSubGRegion = networkModel.Insert[FTN.Common.DMSType.SUBGEOGRAPHICALREGION];

            foreach (var item in ioSubGRegion.Keys)
            {
                DERMSCommon.DataModel.Core.SubGeographicalRegion s = ioSubGRegion[item] as DERMSCommon.DataModel.Core.SubGeographicalRegion;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelioSubstationFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za Substation
            Dictionary<long, IdentifiedObject> ioSubstation = networkModel.Insert[FTN.Common.DMSType.SUBSTATION];

            foreach (var item in ioSubstation.Keys)
            {
                DERMSCommon.DataModel.Core.Substation s = ioSubstation[item] as DERMSCommon.DataModel.Core.Substation;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelBreakerFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za Breaker
            Dictionary<long, IdentifiedObject> ioBreaker = networkModel.Insert[FTN.Common.DMSType.BREAKER];

            foreach (var item in ioBreaker.Keys)
            {
                DERMSCommon.DataModel.Wires.Breaker s = ioBreaker[item] as DERMSCommon.DataModel.Wires.Breaker;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        public void GetNetworkModelACLineSegmentrFromProxy(object parameter)
        {
            NetworkModelTransfer networkModel = (NetworkModelTransfer)parameter;
            List<PointLatLng> coordinates = new List<PointLatLng>(); //Lista koordinata za ACLineSegment
            Dictionary<long, IdentifiedObject> ioACLineSegment = networkModel.Insert[FTN.Common.DMSType.ACLINESEGMENT];

            foreach (var item in ioACLineSegment.Keys)
            {
                DERMSCommon.DataModel.Wires.ACLineSegment s = ioACLineSegment[item] as DERMSCommon.DataModel.Wires.ACLineSegment;
                coordinates.Add(new PointLatLng(s.Latitude, s.Longitude));
            }
        }

        private void GetSignalsFromProxy(object parameter)
        {
            List<DataPoint> newPoints = (List<DataPoint>)parameter;

            foreach (DataPoint dataPoint in newPoints)
            {
                DataPoint item = Points.Where(x => x.Gid == dataPoint.Gid).FirstOrDefault();

                if (item == null)
                {
                    Points.Add(dataPoint);
                }
                else
                {
                    Points.Remove(item);
                    Points.Add(dataPoint);
                }
            }

            Mediator.NotifyColleagues("AlarmSignalUpdate", newPoints);
            Mediator.NotifyColleagues("AllSignalUpdate", newPoints);
        }

        private void GetAlarmSignals(object parameter)
        {
            List<DataPoint> sendPoints = new List<DataPoint>();

            foreach (DataPoint dataPoint in Points)
            {

                if (dataPoint.Alarm !=  AlarmType.NO_ALARM)
                {
                    sendPoints.Add(dataPoint);
                }
            }

            Mediator.NotifyColleagues("AlarmSignalUpdate", sendPoints);
        }

        private void GetAllSignals(object parameter)
        {
            Mediator.NotifyColleagues("AllSignalUpdate", Points);
        }
        #endregion

        #region TreeView Commands
        public List<EnergyNetwork> EnergyNetworks
        {
            get
            {
                return energyNetworks;
            }
            set
            {
                energyNetworks = value;
                OnPropertyChanged("EnergyNetworks");
            }
        }

        public void myEnergyNetworkCommand(long par)
        {
            Console.Beep();
        }
        public void myGeographicalRegionCommand(long par)
        {
            Console.Beep();
        }
        public void mySubstationCommand(long par)
        {
            Console.Beep();
        }
        public void myFeederCommand(long par)
        {
            Console.Beep();
        }
        #endregion

        #region Model Management 
        public IEnumerable<SampleGroupVm> Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                OnPropertyChanged("Samples");
            }
        }
        public UserControl Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged("Content");
            }
        }
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                _isMenuOpen = value;
                OnPropertyChanged("IsMenuOpen");
            }
        }
        public string Criteria
        {
            get { return _criteria; }
            set
            {
                _criteria = value;
                OnPropertyChanged("Criteria");
                OnCriteriaChanged();
            }
        }
        #endregion

        #region Commanding
        public IEnumerable<SampleGroupVm> Samples2
        {
            get { return _samples2; }
            set
            {
                _samples2 = value;
                OnPropertyChanged("Samples2");
            }
        }
        public UserControl Content2
        {
            get { return _content2; }
            set
            {
                _content2 = value;
                OnPropertyChanged("Content2");
            }
        }
        public bool IsMenuOpen2
        {
            get { return _isMenuOpen2; }
            set
            {
                _isMenuOpen2 = value;
                OnPropertyChanged("IsMenuOpen2");
            }
        }
        public string Criteria2
        {
            get { return _criteria2; }
            set
            {
                _criteria2 = value;
                OnPropertyChanged("Criteria2");
                OnCriteriaChanged();
            }
        }
        #endregion

        #region Monitoring
        public IEnumerable<SampleGroupVm> Samples3
        {
            get { return _samples3; }
            set
            {
                _samples3 = value;
                OnPropertyChanged("Samples3");
            }
        }
        public UserControl Content3
        {
            get { return _content3; }
            set
            {
                _content3 = value;
                OnPropertyChanged("Content3");
            }
        }
        public bool IsMenuOpen3
        {
            get { return _isMenuOpen3; }
            set
            {
                _isMenuOpen3 = value;
                OnPropertyChanged("IsMenuOpen3");
            }
        }
        public string Criteria3
        {
            get { return _criteria3; }
            set
            {
                _criteria3 = value;
                OnPropertyChanged("Criteria3");
                OnCriteriaChanged();
            }
        }
        #endregion

        #region Loggs
        public IEnumerable<SampleGroupVm> Samples4
        {
            get { return _samples4; }
            set
            {
                _samples4 = value;
                OnPropertyChanged("Samples4");
            }
        }
        public UserControl Content4
        {
            get { return _content4; }
            set
            {
                _content4 = value;
                OnPropertyChanged("Content4");
            }
        }
        public bool IsMenuOpen4
        {
            get { return _isMenuOpen4; }
            set
            {
                _isMenuOpen4 = value;
                OnPropertyChanged("IsMenuOpen4");
            }
        }
        public string Criteria4
        {
            get { return _criteria4; }
            set
            {
                _criteria4 = value;
                OnPropertyChanged("Criteria4");
                OnCriteriaChanged();
            }
        }
        #endregion

        #region Helper
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCriteriaChanged()
        {
            if (string.IsNullOrWhiteSpace(Criteria))
            {
                Samples = _dataSource;
                return;
            }

            Samples = Samples.Select(x => new SampleGroupVm
            {
                Name = x.Name,
                Items = x.Items.Where(y => y.Title.ToLowerInvariant().Contains(Criteria.ToLowerInvariant()) ||
                                           y.Tags.ToLowerInvariant().Contains(Criteria.ToLowerInvariant()))
            });
        }
        #endregion
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
