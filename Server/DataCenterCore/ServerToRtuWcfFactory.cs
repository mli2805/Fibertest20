using System;
using System.ServiceModel;
using DataCenterCore.RtuWcfServiceReference;

namespace DataCenterCore
{
    public static class ServerToRtuWcfFactory
    {
        public static RtuWcfServiceClient Create(string address)
        {
            try
            {
                var wcfConnection = new RtuWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11842, @"RtuWcfService"))));
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

//    public static class WcfConnectionFactory
//    {
//        public static T Create<T>(string address, int tcpPort, string wcfServiceName)
//        {
//            try
//            {
//                var wcfConnection = new T(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11842, @"ClientWcfService"))));
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                throw;
//            }
//        }
//        private static string CombineUriString(string address, int port, string wcfServiceName)
//        {
//            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
//        }
//        private static NetTcpBinding CreateDefaultNetTcpBinding()
//        {
//            return new NetTcpBinding
//            {
//                Security = { Mode = SecurityMode.None },
//                ReceiveTimeout = new TimeSpan(0, 15, 0),
//                SendTimeout = new TimeSpan(0, 15, 0),
//                OpenTimeout = new TimeSpan(0, 1, 0),
//                MaxBufferSize = 4096000 //4M
//            };
//        }
//    }
}
