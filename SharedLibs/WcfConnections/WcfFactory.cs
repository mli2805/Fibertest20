using System;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.WcfConnections
{
    public sealed class MyClient<T> : ClientBase<T> where T : class
    {
        public MyClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }
    }

    public class WcfFactory
    {
        private readonly DoubleAddress _endPoint;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public WcfFactory(DoubleAddress endPoint, IniFile iniFile, IMyLog logFile)
        {
            _endPoint = endPoint;
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public IClientWcfService CreateClientConnection()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IClientWcfService>(
                    CreateDefaultNetTcpBinding(_iniFile), 
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(),netAddress.Port, @"ClientWcfService"))));
                return myClient.ChannelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

//        private MyClient<IWcfServiceForClient> _c2DClient;
        public IWcfServiceForClient CreateC2DConnection()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                                var myClient = new MyClient<IWcfServiceForClient>(
//                _c2DClient = new MyClient<IWcfServiceForClient>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForClient"))));

                return myClient.ChannelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

     
        public IWcfServiceForRtu CreateR2DConnection(bool shouldWriteToLogProblems = true)
        {
            var netAddress = SelectNetAddressAvailableNow(shouldWriteToLogProblems);
            if (netAddress == null)
                return null;

            try
            {
                var myClient = new MyClient<IWcfServiceForRtu>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForRtu"))));
                return myClient.ChannelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Exception while RTU to Server {netAddress.ToStringASpace} connection creating...");
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public IRtuWcfService CreateRtuConnection()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IRtuWcfService>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"RtuWcfService"))));
                return myClient.ChannelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public IRtuWcfService CreateDuplexRtuConnection(RtuWcfServiceBackward backward)
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new DuplexChannelFactory<IRtuWcfService>(
                    backward,
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"RtuWcfService"))));
                return myClient.CreateChannel();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        private NetAddress SelectNetAddressAvailableNow(bool shouldWriteToLogProblems = true)
        {
            var openTimeout = TimeSpan.FromMilliseconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.OpenTimeoutMs, 1000));

            try
            {
                if (CheckTcpConnection(_endPoint.Main, openTimeout, shouldWriteToLogProblems))
                    return _endPoint.Main;

                if (_endPoint.HasReserveAddress)
                {
                    if (CheckTcpConnection(_endPoint.Reserve, openTimeout, shouldWriteToLogProblems))
                        return _endPoint.Reserve;
                }
            }
            catch (Exception e)
            {
                if (shouldWriteToLogProblems)
                {
                    _logFile.AppendLine("Exception while available address selection...");
                    _logFile.AppendLine(e.Message);
                }
                return null;
            }

            return null;
        }

        private bool CheckTcpConnection(NetAddress netAddress, TimeSpan openTimeout, bool shouldWriteToLogProblems)
        {
            var tcpClient = new TcpClient();

            try
            {
                var tcpConnection = tcpClient.BeginConnect(netAddress.GetAddress(), netAddress.Port, null, null);
                if (tcpConnection.AsyncWaitHandle.WaitOne(openTimeout))
                {
                    tcpClient.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }

            if (shouldWriteToLogProblems)
            {
                var pingTimeout = _iniFile.Read(IniSection.NetTcpBinding, IniKey.PingTimeoutMs, 120);
                var isPingPassed = Pinger.Ping(netAddress.GetAddress(), pingTimeout);
                var word = isPingPassed ? "passed" : $"failed (Timeout is {pingTimeout} ms)";

                _logFile.AppendLine(
                    $"Can't connect to {netAddress.ToStringA()} (Timeout {openTimeout.TotalMilliseconds} ms), ping {word}");
                if (isPingPassed)
                    _logFile.AppendLine("Check that on another end service started and versions match");
            }
            return false;
        }

        public static string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

        public static NetTcpBinding CreateDefaultNetTcpBinding(IniFile iniFile)
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                OpenTimeout = TimeSpan.FromSeconds(20),
                
                ReceiveTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.ReadTimeout, 60)),
                SendTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.SendTimeout, 60)),
                ReaderQuotas = { MaxArrayLength = 40960000},
                MaxBufferSize = 40960000, //4M
                MaxReceivedMessageSize = 40960000,
            };
        }

    }
}
