using System;
using System.Net.Sockets;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.StringResources;
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
                var connection = new ClientWcfServiceClient(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(_endPoint.Main.Ip4Address, TcpPorts.ClientListenTo, @"ClientWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint.Main.Ip4Address, (int)TcpPorts.ClientListenTo));
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForClientClient CreateC2DConnection()
        {
            try
            {
                var connection = new WcfServiceForClientClient(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(_endPoint.Main.Ip4Address, TcpPorts.ServerListenToClient, @"WcfServiceForClient"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint.Main.Ip4Address, (int)TcpPorts.ServerListenToClient));
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForRtuClient CreateR2DConnection()
        {
            try
            {
                var openTimeout = TimeSpan.FromSeconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.OpenTimeout, 0.5));
                var tcpClient = new TcpClient();
                var tcpConnection = tcpClient.BeginConnect(_endPoint.Main.Ip4Address, (int)TcpPorts.ServerListenToRtu, null, null);
                var success = tcpConnection.AsyncWaitHandle.WaitOne(openTimeout);
                if (!success)
                {
                    _logFile.AppendLine($"Can't connect to {_endPoint.Main.Ip4Address}:{(int)TcpPorts.ServerListenToRtu} (Open timeout {openTimeout.Seconds} s)");
                    var pingTimeout = _iniFile.Read(IniSection.NetTcpBinding, IniKey.PingTimeout, 120);
                    var word = Pinger.Ping(_endPoint.Main.Ip4Address, pingTimeout) ? "passed" : $"failed (timeout is {pingTimeout} ms)";
                    _logFile.AppendLine($"Ping {_endPoint.Main.Ip4Address} {word}");
                    return null;
                }

                var connection = new WcfServiceForRtuClient(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(_endPoint.Main.Ip4Address, TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint.Main.Ip4Address, (int)TcpPorts.ServerListenToRtu));
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public RtuWcfServiceClient CreateRtuConnection()
        {
            try
            {
                var connection = new RtuWcfServiceClient(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(_endPoint.Main.Ip4Address, TcpPorts.RtuListenTo, @"RtuWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint.Main.Ip4Address, (int)TcpPorts.RtuListenTo));
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        private string CombineUriString(string address, TcpPorts port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + (int)port + @"/" + wcfServiceName;
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
