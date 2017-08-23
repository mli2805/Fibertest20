using System;
using System.Net.Sockets;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections.C2DWcfServiceReference;
using WcfConnections.ClientWcfServiceReference;
using WcfConnections.R2DWcfServiceReference;
using WcfConnections.RtuWcfServiceReference;

namespace WcfConnections
{
    public class WcfFactory
    {
        private readonly DoubleAddressWithLastConnectionCheck _endPoint;
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;

        public WcfFactory(DoubleAddressWithLastConnectionCheck endPoint, IniFile iniFile, LogFile logFile)
        {
            _endPoint = endPoint;
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public ClientWcfServiceClient CreateClientConnection()
        {
            try
            {
                var netAddress = SelectAvailableNetAddress();
                if (netAddress == null)
                    return null;

                var connection =
                    new ClientWcfServiceClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                        new Uri(CombineUriString(netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName, netAddress.Port, @"ClientWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForClientClient CreateC2DConnection()
        {
            try
            {
                var netAddress = SelectAvailableNetAddress();
                if (netAddress == null)
                    return null;

                var connection =
                    new WcfServiceForClientClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                        new Uri(CombineUriString(netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName, netAddress.Port, @"WcfServiceForClient"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForRtuClient CreateR2DConnection()
        {
            try
            {
                var netAddress = SelectAvailableNetAddress();
                if (netAddress == null)
                    return null;

                var connection = 
                     new WcfServiceForRtuClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                            new Uri(CombineUriString(netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName, netAddress.Port, @"WcfServiceForRtu"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public RtuWcfServiceClient CreateRtuConnection()
        {
            try
            {
                var netAddress = SelectAvailableNetAddress();
                if (netAddress == null)
                    return null;

                var connection =
                    new RtuWcfServiceClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                        new Uri(CombineUriString(netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName, netAddress.Port, @"RtuWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        private NetAddress SelectAvailableNetAddress()
        {
            var openTimeout = TimeSpan.FromMilliseconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.OpenTimeoutMs, 1000));

            if (CheckTcpConnection(_endPoint.Main, openTimeout))
                return _endPoint.Main;

            if (_endPoint.HasReserveAddress)
            {
                if (CheckTcpConnection(_endPoint.Reserve, openTimeout))
                    return _endPoint.Reserve;
            }

            return null;
        }

        private bool CheckTcpConnection(NetAddress netAddress, TimeSpan openTimeout)
        {
            var tcpClient = new TcpClient();

            var address = netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : netAddress.HostName;
            var tcpConnection = tcpClient.BeginConnect(address, netAddress.Port, null, null);
            if (tcpConnection.AsyncWaitHandle.WaitOne(openTimeout))
                return true;

            _logFile.AppendLine($"Can't connect to {address}:{netAddress.Port} (Timeout {openTimeout.TotalMilliseconds} ms)");
            var pingTimeout = _iniFile.Read(IniSection.NetTcpBinding, IniKey.PingTimeoutMs, 120);
            var word = Pinger.Ping(address, pingTimeout) ? "passed" : $"failed (Timeout is {pingTimeout} ms)";
            _logFile.AppendLine($"Ping {address} {word}");

            return false;
        }

        private string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

        private NetTcpBinding CreateDefaultNetTcpBinding(IniFile iniFile)
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.ReadTimeout, 3)),
                SendTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.SendTimeout, 3)),
                MaxBufferSize = 4096000 //4M
            };
        }

    }
}
