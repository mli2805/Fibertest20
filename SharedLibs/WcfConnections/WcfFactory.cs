using System;
using System.Net.Sockets;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;
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
        private readonly Logger35 _logger35;

        public WcfFactory(DoubleAddressWithLastConnectionCheck endPoint, IniFile iniFile, Logger35 logger35)
        {
            _endPoint = endPoint;
            _iniFile = iniFile;
            _logger35 = logger35;
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
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint, (int)TcpPorts.ClientListenTo));
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint, (int)TcpPorts.ServerListenToClient));
                _logger35.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForRtuClient CreateR2DConnection()
        {
            try
            {
                var openTimeout = TimeSpan.FromSeconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.OpenTimeout, 1));
                var tcpClient = new TcpClient();
                var tcpConnection = tcpClient.BeginConnect(_endPoint.Main.Ip4Address, (int)TcpPorts.ServerListenToRtu, null, null);
                var success = tcpConnection.AsyncWaitHandle.WaitOne(openTimeout);
                if (!success)
                {
                    _logger35.AppendLine($"Can't establish connection with {_endPoint}:{(int)TcpPorts.ServerListenToRtu}");
                    var word = Pinger.Ping(_endPoint.Main.Ip4Address) ? "passed" : "failed";
                    _logger35.AppendLine($"Ping {_endPoint} {word}");
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
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint, (int)TcpPorts.ServerListenToRtu));
                _logger35.AppendLine(e.Message);
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
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPoint, (int)TcpPorts.RtuListenTo));
                _logger35.AppendLine(e.Message);
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
