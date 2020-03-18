using CalculationEngineServiceCommon;
using Common;
using dCom.Exceptions;
using DERMSCommon.NMSCommuication;
using DERMSCommon.TransactionManager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace dCom.Configuration
{
	internal class ConfigReader : IConfiguration
	{

        private ServiceHost serviceHostForNMS; 
        private ServiceHost serviceHostForCE; 
		private ushort transactionId = 0;

		private byte unitAddress;
		private int tcpPort;
		private ConfigItemEqualityComparer confItemEqComp = new ConfigItemEqualityComparer();

		private Dictionary<string, IConfigItem> pointTypeToConfiguration = new Dictionary<string, IConfigItem>(30);

		private string path = "RtuCfg.txt";

		public ConfigReader()
		{
			if (!File.Exists(path))
			{
				OpenConfigFile();
			}

			ReadConfiguration();
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

		private void ReadConfiguration()
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
						continue;
					}
                    try
                    {
                        ConfigItem ci = new ConfigItem(filtered);
                        if (pointTypeToConfiguration.Count > 0)
                        {
                            foreach (ConfigItem cf in pointTypeToConfiguration.Values)
                            {
                                if (!confItemEqComp.Equals(cf, ci))
                                {
                                    pointTypeToConfiguration.Add(ci.Description, ci);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            pointTypeToConfiguration.Add(ci.Description, ci);
                        }
                    }
                    catch (ArgumentException argEx)
                    {
                        throw new ConfigurationException($"Configuration error: {argEx.Message}", argEx);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
				if (pointTypeToConfiguration.Count == 0)
				{
					throw new ConfigurationException("Configuration error! Check RtuCfg.txt file!");
				}
			}

            //

            //Open service for NMS
            string address3 = String.Format("net.tcp://localhost:19012/ISendDataFromNMSToScada");
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
            serviceHostForNMS = new ServiceHost(typeof(SendDataFromNmsToScada));

            serviceHostForNMS.AddServiceEndpoint(typeof(ISendDataFromNMSToScada), binding, address3);
            serviceHostForNMS.Open();
            Console.WriteLine("Open: net.tcp://localhost:19012/ISendDataFromNMSToScada");

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

			//Open service for CE
			string address2 = String.Format("net.tcp://localhost:18503/ISendListOfGeneratorsToScada");
			NetTcpBinding binding2 = new NetTcpBinding();
			binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
			serviceHostForCE = new ServiceHost(typeof(SendListOfGeneratorsToScada));
			serviceHostForCE.AddServiceEndpoint(typeof(ISendListOfGeneratorsToScada), binding2, address2);
			serviceHostForCE.Open();
			Console.WriteLine("Open: net.tcp://localhost:18503/ISendListOfGeneratorsToScada");
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