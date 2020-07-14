using Common;
using dCom.Exceptions;
using DERMSCommon.DataModel.Core;
using DERMSCommon.DataModel.Meas;
using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using Microsoft.Win32;
using Modbus;
using Modbus.FunctionParameters;
using ProcessingModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.ServiceModel;

namespace dCom.Configuration
{
    internal class ConfigReader : SCADACommunication, IConfiguration
    {
        private int AnalogPoints = 0;
        private int DigitalPoints = 0;
        private ServiceHost serviceHostForNMS;
        private ushort transactionId = 0;
        private byte unitAddress;
        private int tcpPort;
        private ConfigItemEqualityComparer confItemEqComp = new ConfigItemEqualityComparer();
        private Dictionary<string, IConfigItem> pointTypeToConfiguration = new Dictionary<string, IConfigItem>(30);
        private string path = "RtuCfg.txt";

        public ConfigReader(Dictionary<long, IdentifiedObject> analogni, Dictionary<long, IdentifiedObject> digitalni)
        {
            if (!File.Exists(path))
            {
                OpenConfigFile();
            }

            ReadConfiguration(analogni, digitalni);
        }
        public ConfigReader()
        {
            Dictionary<long, IdentifiedObject> analogni = new Dictionary<long, IdentifiedObject>();

            Dictionary<long, IdentifiedObject> digitalni = new Dictionary<long, IdentifiedObject>();
            if (!File.Exists(path))
            {
                OpenConfigFile();
            }

            ReadConfiguration(analogni, digitalni);
        }
        public int GetAcquisitionInterval(string pointDescription)
        {
            IConfigItem ci;
            if (pointTypeToConfiguration.TryGetValue(pointDescription, out ci))
            {
                return ci.AcquisitionInterval;
            }
            throw new ArgumentException(string.Format("Invalid argument:{0}", nameof(pointDescription)));
        }

        public ushort GetStartAddress(string pointDescription)
        {
            IConfigItem ci;
            if (pointTypeToConfiguration.TryGetValue(pointDescription, out ci))
            {
                return ci.StartAddress;
            }
            throw new ArgumentException(string.Format("Invalid argument:{0}", nameof(pointDescription)));
        }

        public ushort GetNumberOfRegisters(string pointDescription)
        {
            IConfigItem ci;
            if (pointTypeToConfiguration.TryGetValue(pointDescription, out ci))
            {
                return ci.NumberOfRegisters;
            }
            throw new ArgumentException(string.Format("Invalid argument:{0}", nameof(pointDescription)));
        }

        private void OpenConfigFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.FileOk += Dlg_FileOk;
            dlg.ShowDialog();
        }

        private void Dlg_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            path = (sender as OpenFileDialog).FileName;
        }

        private void ReadConfiguration(Dictionary<long, IdentifiedObject> analogni, Dictionary<long, IdentifiedObject> digitalni)
        {
          
            using (TextReader tr = new StreamReader(path))
            {
                string s = string.Empty;
                while ((s = tr.ReadLine()) != null)
                {
                    string[] splited = s.Split(' ', '\t');
                    List<string> filtered = splited.ToList().FindAll(t => !string.IsNullOrEmpty(t));
                    if (filtered.Count == 0)
                    {
                        continue;
                    }
                    if (s.StartsWith("STA"))
                    {
                        unitAddress = Convert.ToByte(filtered[filtered.Count - 1]);
                        continue;
                    }
                    if (s.StartsWith("TCP"))
                    {
                        TcpPort = Convert.ToInt32(filtered[filtered.Count - 1]);
                        break;
                    }
                }
            }
            foreach (KeyValuePair<long, IdentifiedObject> analog in analogni)
            {
                ConfigItem configItem = new ConfigItem();
                configItem.RegistryType = PointType.ANALOG_OUTPUT;
                configItem.MaxValue = (Int32)((Analog)analog.Value).MaxValue;
                configItem.MaxValue = (Int32)((Analog)analog.Value).MinValue;
                configItem.DefaultValue = (Int32)((Analog)analog.Value).NormalValue;
                configItem.ProcessingType = ((Analog)analog.Value).Name;
                configItem.Description = ((Analog)analog.Value).Description;

                configItem.ScaleFactor = 5;
                configItem.Deviation = 2;
                configItem.EGU_Min = (Int32)((Analog)analog.Value).MinValue - 5;
                configItem.EGU_Max = (Int32)((Analog)analog.Value).MaxValue + 30;
                configItem.HighAlarm = (Int32)((Analog)analog.Value).MaxValue;
                configItem.LowAlarm = (Int32)((Analog)analog.Value).MinValue;
                configItem.Gid = analog.Value.GlobalId;
                configItem.GidGeneratora = ((Analog)analog.Value).PowerSystemResource;
                configItem.NumberOfRegisters = 0;
                configItem.StartAddress = (ushort)(3000 + AnalogPoints);
                AnalogPoints++;
                pointTypeToConfiguration.Add(configItem.Gid.ToString(), configItem);
                List<long> Gidovi = new List<long>();
                Gidovi.Add(((Analog)analog.Value).PowerSystemResource);
                Gidovi.Add(configItem.Gid);
                if (configItem.Description == "Commanding")
                {
                    Gidovi.Add(1);
   
                }
                else if (configItem.Description == "Simulation")
                {
                    Gidovi.Add(2);
                }
                GidoviNaAdresu.Add(Gidovi, configItem.StartAddress);


            }
            foreach (KeyValuePair<long, IdentifiedObject> digital in digitalni)
            {
                ConfigItem configItem = new ConfigItem();
                configItem.RegistryType = PointType.DIGITAL_OUTPUT;
                configItem.MaxValue = ((Discrete)digital.Value).MaxValue;
                configItem.MinValue = ((Discrete)digital.Value).MinValue;
                configItem.DefaultValue = ((Discrete)digital.Value).NormalValue;
                configItem.ProcessingType = ((Discrete)digital.Value).Name;
                configItem.Description = ((Discrete)digital.Value).Description;

                configItem.NumberOfRegisters = 0;
                configItem.StartAddress = (ushort)(40 + DigitalPoints);
                DigitalPoints++;
                configItem.Gid = ((Discrete)digital.Value).GlobalId;
                pointTypeToConfiguration.Add(configItem.Gid.ToString(), configItem);
                configItem.GidGeneratora = ((Discrete)digital.Value).PowerSystemResource;
                List<long> Gidovi = new List<long>();
                Gidovi.Add(((Discrete)digital.Value).PowerSystemResource);
                Gidovi.Add(configItem.Gid);
                if (configItem.Description == "Commanding")
                {
                    Gidovi.Add(1);
                }
                GidoviNaAdresu.Add(Gidovi, configItem.StartAddress);


            }
            //try
            //{
            //    ConfigItem ci = new ConfigItem(filtered);
            //    if (pointTypeToConfiguration.Count > 0)
            //    {
            //        foreach (ConfigItem cf in pointTypeToConfiguration.Values)
            //        {
            //            if (!confItemEqComp.Equals(cf, ci))
            //            {
            //                pointTypeToConfiguration.Add(ci.Description, ci);
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        pointTypeToConfiguration.Add(ci.Description, ci);
            //    }
            //}
            //catch (ArgumentException argEx)
            //{
            //    throw new ConfigurationException($"Configuration error: {argEx.Message}", argEx);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        public ushort GetTransactionId()
        {
            return transactionId++;
        }

        public byte UnitAddress
        {
            get
            {
                return unitAddress;
            }

            private set
            {
                unitAddress = value;
            }
        }

        public int TcpPort
        {
            get
            {
                return tcpPort;
            }

            private set
            {
                tcpPort = value;
            }
        }

        public List<IConfigItem> GetConfigurationItems()
        {
            return new List<IConfigItem>(pointTypeToConfiguration.Values);
        }
    }
}