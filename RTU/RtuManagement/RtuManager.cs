using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private Guid _id;
        private readonly string _version;
        private readonly IMyLog _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly IMyLog _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;
        private CancellationTokenSource _cancellationTokenSource;

        public BaseRefsSaver BaseRefsSaver { get; set; }

        private readonly object _isMonitoringOnLocker = new object();
        private bool _isMonitoringOn;
        public bool IsMonitoringOn
        {
            get { lock (_isMonitoringOnLocker) { return _isMonitoringOn; } }
            set { lock (_isMonitoringOnLocker) { _isMonitoringOn = value; } }
        }
        private bool _wasMonitoringOn;

        private readonly object _lastSuccessfullMeasTimestampLocker = new object();
        private DateTime _lastSuccessfullMeasTimestamp;
        public DateTime LastSuccessfullMeasTimestamp
        {
            get
            {
                lock (_lastSuccessfullMeasTimestampLocker)
                {
                    return _lastSuccessfullMeasTimestamp;
                }
            }
            set
            {
                lock (_lastSuccessfullMeasTimestampLocker)
                {
                    _lastSuccessfullMeasTimestamp = value;
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

        private ReturnCode _rtuInitializationResult;

        public RtuManager(IMyLog serviceLog, IniFile serviceIni)
        {
            IsRtuInitialized = false;

            _serviceLog = serviceLog;
            _serviceIni = serviceIni;
            _serverAddresses = _serviceIni.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);

            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");

            _rtuLog = new LogFile(_rtuIni).AssignFile("RtuManager.log");

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 40));

            _id = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));

            // takes version from RtuManagement.dll
            // mind maintain the same version for all assemblies
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _version = info.FileVersion;
            _serviceIni.Write(IniSection.General, IniKey.Version, _version);

            BaseRefsSaver = new BaseRefsSaver(_rtuIni, _rtuLog);
        }

        public void OnServiceStart()
        {
            Initialize(null, null);
        }

        public RtuInitializedDto GetInitializationResult()
        {
            return new RtuInitializedDto()
            {
                RtuId = _id,
                IsInitialized = IsRtuInitialized,
                ReturnCode = _rtuInitializationResult,
                Serial = _mainCharon?.Serial,
                FullPortCount = _mainCharon?.FullPortCount ?? 0,
                OwnPortCount = _mainCharon?.OwnPortCount ?? 0,
                Children = _mainCharon?.GetChildrenDto(),
                OtdrAddress = _mainCharon?.NetAddress,
                Version = _version,
                IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0,
                AcceptableMeasParams = _otdrManager.InterOpWrapper.GetTreeOfAcceptableMeasParams(),
            };
        }

    }
}