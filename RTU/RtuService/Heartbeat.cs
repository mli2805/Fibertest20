using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuService
{
    public class Heartbeat
    {
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;

        DoubleAddressWithConnectionStats _serverAddressWithConnectionStats = new DoubleAddressWithConnectionStats();

        public Heartbeat(IniFile serviceIni, IMyLog serviceLog)
        {
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
        }

        public void Start()
        {
            var checkChannelsTimeout =
                TimeSpan.FromSeconds(_serviceIni.Read(IniSection.General, IniKey.CheckChannelsTimeout, 30));
            _serviceLog.AppendLine("Heartbeat started");

            // couldn't be changed in service runtime
                var version = _serviceIni.Read(IniSection.General, IniKey.Version, "2.0.1.0");
            while (true)
            {
                // both could be changed due initialization
                var rtuId = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));
                var currentAddresses = _serviceIni.ReadDoubleAddress((int) TcpPorts.ServerListenToRtu);

                _serverAddressWithConnectionStats.DoubleAddress = (DoubleAddress)currentAddresses.Clone();
                _serverAddressWithConnectionStats = 
                    new R2DWcfManager(_serverAddressWithConnectionStats, _serviceIni, _serviceLog)
                    .SendImAliveByBothChannels(rtuId, version);
                Thread.Sleep(checkChannelsTimeout);
            }
        }

    }
}