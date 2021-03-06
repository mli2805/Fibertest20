﻿using System;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class MyClient<T> : ClientBase<T> where T : class
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

     
        public ChannelFactory<IWcfServiceInClient> GetClientChannelFactory()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow(false);
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IWcfServiceInClient>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"ClientWcfService"))));
                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public ChannelFactory<IWcfServiceDesktopC2D> GetDesktopC2DChannelFactory()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IWcfServiceDesktopC2D>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForDesktopC2D"))));

                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public ChannelFactory<IWcfServiceCommonC2D> GetCommonC2DChannelFactory()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IWcfServiceCommonC2D>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForCommonC2D"))));

                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public ChannelFactory<IWcfServiceWebC2D> GetWebC2DChannelFactory()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IWcfServiceWebC2D>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForWebC2D"))));

                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public ChannelFactory<IWcfServiceInSuperClient> GetC2SChannelFactory()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var myClient = new MyClient<IWcfServiceInSuperClient>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceInSuperClient"))));

                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }

        }

        public ChannelFactory<IWcfServiceForRtu> GetR2DChannelFactory(bool shouldWriteToLogProblems = true)
        {
            var netAddress = SelectNetAddressAvailableNow(shouldWriteToLogProblems);
            if (netAddress == null)
                return null;

            try
            {
                var myClient = new MyClient<IWcfServiceForRtu>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForRtu"))));
                return myClient.ChannelFactory;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"Exception while RTU to Server {netAddress.ToStringASpace} connection creating...");
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public ChannelFactory<IRtuWcfService> GetDuplexRtuChannelFactory(RtuWcfServiceBackward backward)
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;

                var factory = new DuplexChannelFactory<IRtuWcfService>(
                    backward,
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"RtuWcfService"))));
                return factory;
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

                ReceiveTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.ReadTimeout, 140)),
                SendTimeout = TimeSpan.FromSeconds(iniFile.Read(IniSection.NetTcpBinding, IniKey.SendTimeout, 140)),
                ReaderQuotas = { MaxArrayLength = 40960000 },
                MaxBufferSize = 40960000, //4M
                MaxReceivedMessageSize = 40960000,
            };
        }

    }
}
