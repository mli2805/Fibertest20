using System;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public class WoodPecker
    {
        private readonly Guid _rtuId;
        private readonly string _version;
        private DoubleAddressWithConnectionStats _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly LogFile _serviceLog;

        public WoodPecker(Guid rtuId, string version, DoubleAddressWithConnectionStats serverAddresses, IniFile serviceIni, LogFile serviceLog)
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
            while (true)
            {
                _serverAddresses = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendImAliveByBothChannels(_rtuId, _version);
                Thread.Sleep(checkChannelsTimeout);
            }
        }

    }
}