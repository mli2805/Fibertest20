using System;
using System.ServiceModel;
using DataCenterCore.ClientWcfServiceReference;

namespace DataCenterCore
{
    public static class ServerToClientWcfFactory
    {
        public static ClientWcfServiceClient Create(string address)
        {
            try
            {
                var wcfConnection = new ClientWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11843, @"ClientWcfService"))));
                wcfConnection.Open();
                return wcfConnection;
            }
            catch (Exception)
            {
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