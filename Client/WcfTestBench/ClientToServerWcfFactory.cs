using System;
using System.ServiceModel;
using System.Windows;
using WcfTestBench.WcfForClientServiceReference;

namespace WcfTestBench
{
    public static class ClientToServerWcfFactory
    {
        public static WcfServiceForClientClient Create(string address)
        {
            try
            {
                var wcfClient = new WcfServiceForClientClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11840, @"WcfServiceForClient"))));
                wcfClient.Open();
                return wcfClient;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"WcfServiceForClient creation error!");
                return null;
            }
        }
        private static string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }
        private static NetTcpBinding CreateDefaultNetTcpBinding()
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = new TimeSpan(0, 15, 0),
                SendTimeout = new TimeSpan(0, 15, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                MaxBufferSize = 4096000 //4M
            };
        }
    }
}
