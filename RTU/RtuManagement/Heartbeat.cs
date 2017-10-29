using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public class Heartbeat
    {
        private readonly Guid _rtuId;
        private readonly string _version;
        private DoubleAddressWithConnectionStats _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;

       

        public Heartbeat(Guid rtuId, string version, DoubleAddressWithConnectionStats serverAddresses, IniFile serviceIni, IMyLog serviceLog)
        {
            _rtuId = rtuId;
            _version = version;
            _serverAddresses = serverAddresses;
            _serviceIni = serviceIni;
            _serviceLog = serviceLog;
        }

        public void Start()
        {
            var checkChannelsTimeout =
                TimeSpan.FromSeconds(_serviceIni.Read(IniSection.General, IniKey.CheckChannelsTimeout, 30));
            _serviceLog.AppendLine("Heartbeat started");
            while (true)
            {
                var a = _serviceIni.ReadDoubleAddress((int) TcpPorts.ServerListenToRtu);
                _serverAddresses.DoubleAddress = (DoubleAddress)a.Clone();
                _serverAddresses = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendImAliveByBothChannels(_rtuId, _version);
                Thread.Sleep(checkChannelsTimeout);
            }
        }

    }
}