using System;
using System.Collections.Concurrent;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private readonly Guid _id;
        private readonly LogFile _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly LogFile _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        public ConcurrentQueue<MoniResultOnDisk> QueueOfMoniResultsOnDisk { get; set; }
        private object WcfParameter { get; set; }

        private bool _isMonitoringCancelled;
        private readonly object _isMonitoringCancelledLocker = new object();

        private readonly object _isMonitoringOnLocker = new object();
        private bool _isMonitoringOn;
        public bool IsMonitoringOn
        {
            get
            {
                lock (_isMonitoringOnLocker)
                {
                    return _isMonitoringOn;
                }
            }
            set
            {
                lock (_isMonitoringOnLocker)
                {
                    _isMonitoringOn = value;
                }
            }
        }

        private readonly object _isRtuInitializedLocker = new object();
        private bool _isRtuInitialized;

        public bool IsRtuInitialized
        {
            get
            {
                lock (_isRtuInitializedLocker)
                {
                    return _isRtuInitialized;
                }
            }
            set
            {
                lock (_isRtuInitializedLocker)
                {
                    _isRtuInitialized = value;
                }
            }
        }

        public RtuManager(LogFile serviceLog, IniFile serviceIni)
        {
            IsRtuInitialized = false;

            _serviceLog = serviceLog;
            _serviceIni = serviceIni;
            _serverAddresses =  new DoubleAddressWithConnectionStats() {DoubleAddress = _serviceIni.ReadServerAddresses(), }; 

            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var logFileSizeLimit = _rtuIni.Read(IniSection.General, IniKey.LogFileSizeLimitKb, 0);

            _rtuLog = new LogFile();
            _rtuLog.AssignFile("RtuManager.log", logFileSizeLimit, cultureString);

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 40));
            _id = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));
        }
    }
}