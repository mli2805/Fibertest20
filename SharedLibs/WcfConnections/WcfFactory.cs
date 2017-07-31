using System;
using System.ServiceModel;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;
using WcfConnections.C2DWcfServiceReference;
using WcfConnections.R2DWcfServiceReference;
using WcfConnections.Service_References.ClientWcfServiceReference;
using WcfConnections.Service_References.RtuWcfServiceReference;

namespace WcfConnections
{
    public class WcfFactory
    {
        private readonly string _endPointAddress;
        private readonly IniFile _iniFile;
        private readonly Logger35 _logger35;

        public WcfFactory(string endPointAddress, IniFile iniFile, Logger35 logger35)
        {
            _endPointAddress = endPointAddress;
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
                        new Uri(CombineUriString(_endPointAddress, TcpPorts.ClientListenTo, @"ClientWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPointAddress, (int)TcpPorts.ClientListenTo));
                _logger35.AppendLine(Resources.SID_WcfServiceForClient_creation_error_);
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
                        new Uri(CombineUriString(_endPointAddress, TcpPorts.ServerListenToClient, @"WcfServiceForClient"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPointAddress, (int)TcpPorts.ServerListenToClient));
                _logger35.AppendLine(Resources.SID_WcfServiceForClient_creation_error_);
                _logger35.AppendLine(e.Message);
                return null;
            }
        }

        public WcfServiceForRtuClient CreateR2DConnection()
        {
            try
            {
                var connection = new WcfServiceForRtuClient(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(_endPointAddress, TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPointAddress, (int)TcpPorts.ServerListenToRtu));
                _logger35.AppendLine(Resources.SID_WcfServiceForClient_creation_error_);
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
                        new Uri(CombineUriString(_endPointAddress, TcpPorts.RtuListenTo, @"RtuWcfService"))));
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                _logger35.AppendLine(string.Format(Resources.SID_Cannot_establish_connection_with__0___1_, _endPointAddress, (int)TcpPorts.RtuListenTo));
                _logger35.AppendLine(Resources.SID_WcfServiceForClient_creation_error_);
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
                ReceiveTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.ReadTimeout, 15)),
                SendTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.SendTimeout, 15)),
                OpenTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.OpenTimeout, 5)),
                MaxBufferSize = 4096000 //4M
            };
        }

    }
}
