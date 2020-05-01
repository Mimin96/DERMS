﻿using CalculationEngineServiceCommon;
using DERMSCommon.UIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Resources;

namespace UI.ViewModel
{
    public class BreakerControlThroughGISWindowViewModel : BindableBase
    {
        #region Variables
        private IFlexibilityFromUIToCE ProxyCE { get; set; }
        private ChannelFactory<IFlexibilityFromUIToCE> factoryCE;

        private RelayCommand<object> _breakerOnOff;
        public bool Open { get; set; }
        public bool Close { get; set; }
        public long GID { get; set; }
        #endregion
        public BreakerControlThroughGISWindowViewModel()
        {
            Open = false;
            Close = false;
            Connect();
        }

        public ICommand BreakerOnOffCommand
        {
            get
            {
                if (_breakerOnOff == null)
                {
                    _breakerOnOff = new RelayCommand<object>(BreakerOnOff);
                }

                return _breakerOnOff;
            }
        }

        private void BreakerOnOff(object obj)
        {
            //NormalOpen == true znaci da je breaker otvoren

            try
            {
                bool NormalOpen = Open ? true : false;
                ProxyCE.ChangeBreakerStatus(GID, NormalOpen);

                for (int i = 0; i < Application.Current.Windows.Count; i++)
                {
                    if (Application.Current.Windows[i].Name == "BreakerControl")
                    {
                        Application.Current.Windows[i].Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("BreakerControlThroughGISWindowViewModel: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Connect()
        {
            try
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security = new NetTcpSecurity() { Mode = SecurityMode.None };
                binding.CloseTimeout = System.TimeSpan.FromMinutes(20);
                binding.OpenTimeout = System.TimeSpan.FromMinutes(20);
                binding.ReceiveTimeout = System.TimeSpan.FromMinutes(20);
                binding.SendTimeout = System.TimeSpan.FromMinutes(20);
                binding.MaxBufferSize = 8000000;
                binding.MaxReceivedMessageSize = 8000000;
                binding.MaxBufferPoolSize = 8000000;
                factoryCE = new ChannelFactory<IFlexibilityFromUIToCE>(binding, new EndpointAddress("net.tcp://localhost:19011/IFlexibilityFromUIToCE"));
                ProxyCE = factoryCE.CreateChannel();
            }
            catch (Exception e)
            {
                MessageBox.Show("BreakerControlThroughGISWindowViewModel: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}