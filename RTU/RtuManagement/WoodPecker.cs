using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public class WoodPecker
    {
        private readonly Guid _id;
        private readonly Guid _rtuId;
        private readonly string _version;
        private DoubleAddressWithConnectionStats _serverAddresses;
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;

        private readonly object _isCancelledLocker = new object();
        private bool _isCancelled;
        public bool IsCancelled
        {
            get
            {
                lock (_isCancelledLocker)
                {
                    return _isCancelled;
                }
            }
            set
            {
                lock (_isCancelledLocker)
                {
                    _isCancelled = value;
                }
            }
        }

        public WoodPecker(Guid rtuId, string version, DoubleAddressWithConnectionStats serverAddresses, IniFile serviceIni, IMyLog serviceLog)
        {
            _id = Guid.NewGuid();
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
            _serviceLog.AppendLine($"WoodPecker {_id.First6()} is started");
            while (!IsCancelled)
            {
                var a = _serviceIni.ReadDoubleAddress((int) TcpPorts.ServerListenToRtu);
                _serverAddresses.DoubleAddress = (DoubleAddress)a.Clone();
                _serverAddresses = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendImAliveByBothChannels(_rtuId, _version);
                Thread.Sleep(checkChannelsTimeout);
            }
            _serviceLog.AppendLine($"WoodPecker {_id.First6()} is finished");
        }

    }
}