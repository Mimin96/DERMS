using CalculationEngineServiceCommon;
using DERMSCommon.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CalculationEngineService
{
    public class ClientSideCE
    {
        public ChannelFactory<ISendSCADADataToUI> factoryUI;
        public ChannelFactory<ISendNetworkModelToUI> factoryUI_NM;
        public ChannelFactory<IUpdateCommand> factoryScada;

        public ISendSCADADataToUI ProxyUI { get; set; }
        public ISendNetworkModelToUI ProxyUI_NM { get; set; }
        public IUpdateCommand ProxyScada { get; set; }

        private static ClientSideCE instance = null;
        public static ClientSideCE Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new ClientSideCE();
                }
                   
                return instance;

            }
        }

        public ClientSideCE()
        {
            try
            {
                Connect();
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Connect()
        {
            //Connect to UI
            NetTcpBinding binding = new NetTcpBinding();
            factoryUI = new ChannelFactory<ISendSCADADataToUI>(binding, new EndpointAddress("net.tcp://localhost:19119/ISendSCADADataToUI"));
            ProxyUI = factoryUI.CreateChannel();
            Console.WriteLine("Connected: net.tcp://localhost:19119/ISendSCADADataToUI");

            //Connect to UI
            NetTcpBinding binding3 = new NetTcpBinding();
            factoryUI_NM = new ChannelFactory<ISendNetworkModelToUI>(binding3, new EndpointAddress("net.tcp://localhost:18119/ISendNetworkModelToUI"));
            ProxyUI_NM = factoryUI_NM.CreateChannel();
            Console.WriteLine("Connected: net.tcp://localhost:18119/ISendNetworkModelToUI");

            //Connect to Scada
            NetTcpBinding binding2 = new NetTcpBinding();
            factoryScada = new ChannelFactory<IUpdateCommand>(binding2, new EndpointAddress("net.tcp://localhost:18500/IUpdateCommand"));
            ProxyScada = factoryScada.CreateChannel();
            Console.WriteLine("Connected: net.tcp://localhost:18500/IUpdateCommand");

        }

        public void Abort()
        {
            factoryUI.Abort();
            factoryScada.Abort();
        }

        public void Close()
        {
            factoryUI.Close();
            factoryScada.Close();
        }
    }
}
