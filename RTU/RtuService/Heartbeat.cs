using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuManagement;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuService
{
    public class Heartbeat
    {
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;

        private Guid _rtuId;
        private string _version;
        private DoubleAddress _mainAddressOnly;
        private DoubleAddress _reserveAddressOnly;
        private bool _hasReserveAddress;

        public Heartbeat(IniFile serviceIni, IMyLog serviceLog, RtuManager rtuManager)
        {
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
        }

        // ReSharper disable once FunctionNeverReturns 
        public void Start()
        {
            var rtuHeartbeatRate =
                TimeSpan.FromSeconds(_serviceIni.Read(IniSection.General, IniKey.RtuHeartbeatRate, 30));
            _serviceLog.AppendLine($"Heartbeat started with {rtuHeartbeatRate.TotalSeconds} sec rate");

            // couldn't be changed in service runtime
            _version = _serviceIni.Read(IniSection.General, IniKey.Version, "2.0.1.0");

            bool? isLastConnectionOnMainChannelSuccessful = null;
            bool? isLastConnectionOnReserveChannelSuccessful = null;

            while (true)
            {
                while (!_rtuManager.ShouldSendHeartbeat.TryPeek(out object _))
                {
                    Thread.Sleep(3000);
                }

                ReadMutableParameters();

                SendHeartbeatByOneChannel(_mainAddressOnly, true, ref isLastConnectionOnMainChannelSuccessful);
                if (_hasReserveAddress)
                    SendHeartbeatByOneChannel(_reserveAddressOnly, false, ref isLastConnectionOnReserveChannelSuccessful);

                Thread.Sleep(rtuHeartbeatRate);
            }
        }

        private void SendHeartbeatByOneChannel(DoubleAddress oneChannelAddressOnly, bool isMainChannel, ref bool? isLastSuccess)
        {
            var dto = new RtuChecksChannelDto() { RtuId = _rtuId, Version = _version, IsMainChannel = isMainChannel };
            var channel = new R2DWcfManager(oneChannelAddressOnly, _serviceIni, _serviceLog);
            var isSuccess = channel.SendHeartbeat(dto);
            if (isSuccess == isLastSuccess)
                return;

            var channelName = isMainChannel ? "main" : "reserve";
            _serviceLog.AppendLine(isSuccess ? $"Heartbeat sent by {channelName} channel" : $"Can't send heartbeat by {channelName} channel");
            isLastSuccess = isSuccess;
        }

        private void ReadMutableParameters()
        {
            // both could be changed due initialization
            _rtuId = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));
            var currentAddresses = _serviceIni.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
            _mainAddressOnly = new DoubleAddress() { Main = (NetAddress)currentAddresses.Main.Clone() };
            _hasReserveAddress = currentAddresses.HasReserveAddress;
            _reserveAddressOnly = new DoubleAddress() { Main = (NetAddress)currentAddresses.Reserve.Clone() };
        }



    }
}