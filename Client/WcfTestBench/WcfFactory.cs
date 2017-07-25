using System;
using System.ServiceModel;
using System.Windows;
using Iit.Fibertest.Utils35;
using WcfTestBench.RtuWcfServiceReference;
using WcfTestBench.WcfForClientServiceReference;

namespace WcfTestBench
{
    public static class WcfFactory
    {
        public static WcfServiceForClientClient CreateServerConnection(string address)
        {
            try
            {
                var connection = new WcfServiceForClientClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, TcpPorts.ServerListenToClient, @"WcfServiceForClient"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"WcfServiceForClient creation error!");
                return null;
            }
        }

        public static RtuWcfServiceClient CreateRtuConnection(string address)
        {
            try
            {
                var connection = new RtuWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, TcpPorts.RtuListenTo, @"RtuWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"RtuWcfService creation error!");
                return null;
            }
        }

        private static string CombineUriString(string address, TcpPorts port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + (int)port + @"/" + wcfServiceName;
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
