﻿using System;
using System.Net.Sockets;
using System.ServiceModel;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.UtilsLib;
using RtuWcfServiceLibrary;
using WcfServiceForClientLibrary;
using WcfServiceForRtuLibrary;

namespace WcfConnections
{
    public sealed class MyClient<T> : ClientBase<T> where T : class
    { }
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

                var channelFactory = new ChannelFactory<IClientWcfService>(
                    CreateDefaultNetTcpBinding(_iniFile),
                    new EndpointAddress(
                        new Uri(CombineUriString(netAddress.GetAddress(), 
                        netAddress.Port, @"ClientWcfService"))));
                var service = channelFactory.CreateChannel();


                return service;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public IWcfServiceForClient CreateC2DConnection()
        {
            try
            {
                var netAddress = SelectNetAddressAvailableNow();
                if (netAddress == null)
                    return null;


                var myClient = new MyClient<IWcfServiceForClient>();
                myClient.Open();
                var connection = myClient.ChannelFactory
                    .CreateChannel(new EndpointAddress(
                        new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForClient"))));
                //
                //   new WcfServiceForClientClient(
                //       CreateDefaultNetTcpBinding(_iniFile), 
                //       new EndpointAddress(
                //       new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForClient"))));
                //connection.Open();
                return connection;
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
                var myClient = new MyClient<IWcfServiceForRtu>();
                myClient.Open();
                var connection = myClient.ChannelFactory
                    .CreateChannel(new EndpointAddress(new Uri(
                        CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForRtu"))));

                //    var connection = 
                //         new WcfServiceForRtuClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                //                new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"WcfServiceForRtu"))));
                //    connection.Open();
                return connection;
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

                var connection = new MyClient<IRtuWcfService>();
                connection.Open();
                var rtuWcfService = connection.ChannelFactory.CreateChannel(new EndpointAddress(
                    new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"RtuWcfService"))));
                //var connection =
                //    new RtuWcfServiceClient(CreateDefaultNetTcpBinding(_iniFile), new EndpointAddress(
                //        new Uri(CombineUriString(netAddress.GetAddress(), netAddress.Port, @"RtuWcfService"))));
                //connection.Open();
                return rtuWcfService;
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
                    return true;
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
                    _logFile.AppendLine("Check that on other end service started and versions match");
            }
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
