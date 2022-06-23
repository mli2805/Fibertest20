using System;
using System.Diagnostics;
using System.IO;
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
        private readonly string _versionRtuManager;
        private readonly string _versionIitOtdr;
        private readonly IMyLog _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly IMyLog _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;
        private TreeOfAcceptableMeasParams _treeOfAcceptableMeasParams;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly object _isMonitoringOnLocker = new object();
        private bool _isMonitoringOn;
        public bool IsMonitoringOn
        {
            get { lock (_isMonitoringOnLocker) { return _isMonitoringOn; } }
            set { lock (_isMonitoringOnLocker) { _isMonitoringOn = value; } }
        }
        private bool _wasMonitoringOn;


        private readonly object _isAutoBaseModeLocker = new object();
        private bool _isRtuAutoBaseMode;
        public bool IsRtuAutoBaseMode
        {
            get { lock (_isAutoBaseModeLocker) { return _isRtuAutoBaseMode; } }
            set { lock (_isAutoBaseModeLocker) { _isRtuAutoBaseMode = value; } }
        }

        private readonly object _lastSuccessfulMeasTimestampLocker = new object();
        private DateTime _lastSuccessfulMeasTimestamp;
        public DateTime LastSuccessfulMeasTimestamp
        {
            get
            {
                lock (_lastSuccessfulMeasTimestampLocker)
                {
                    return _lastSuccessfulMeasTimestamp;
                }
            }
            set
            {
                lock (_lastSuccessfulMeasTimestampLocker)
                {
                    _lastSuccessfulMeasTimestamp = value;
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

            _rtuLog = new LogFile(_rtuIni, 50000).AssignFile("RtuManager.log");

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 45));

            _id = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));

            // takes version from RtuManagement.dll
            // mind maintain the same version for all assemblies
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            var creationTime = File.GetLastWriteTime(assembly.Location);
            _version = $"{info.FileVersion}";
            _versionRtuManager = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";
            _serviceIni.Write(IniSection.General, IniKey.Version, _version);

            var path = Path.GetDirectoryName(assembly.Location);
            var iitOtdrPath = Path.Combine(path + @"\OtdrMeasEngine\iit_otdr.dll");
            var creationTime2 = File.GetLastWriteTime(iitOtdrPath);
            FileVersionInfo info2 = FileVersionInfo.GetVersionInfo(iitOtdrPath);
            _versionIitOtdr = $"{info2.FileVersion} build {creationTime2:dd/MM/yyyy}";
        }

        public void OnServiceStart()
        {
            _serviceLog.AppendLine($"RTU Manager version {_versionRtuManager}");
            _serviceLog.AppendLine($"iit_otdr.dll version {_versionIitOtdr}");
            var upTime = Utils.GetUpTime();
            _serviceLog.AppendLine($"Windows' UpTime is {upTime}");
            var limit = _serviceIni.Read(IniSection.General, IniKey.RtuUpTimeForAdditionalPause, 100);
            if (TimeSpan.Zero < upTime && upTime < TimeSpan.FromSeconds(limit))
            {
                var delay = _serviceIni.Read(IniSection.General, IniKey.RtuPauseAfterReboot, 20);
                _serviceLog.AppendLine($"Additional pause after RTU restart is {delay} sec");
                Thread.Sleep(TimeSpan.FromSeconds(delay));
            }

            Initialize(null, null);
        }

        public RtuInitializedDto GetInitializationResult()
        {
            var otdrIp = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
            var otdrPort = _rtuIni.Read(IniSection.Charon, IniKey.OtdrPort, 1500);
            return new RtuInitializedDto()
            {
                Maker = RtuMaker.IIT,
                Mfid = _mfid,
                Mfsn = _mfsn,
                Omid = _omid,
                Omsn = _omsn,
                RtuId = _id,
                IsInitialized = IsRtuInitialized,
                ReturnCode = _rtuInitializationResult,
                Serial = _mainCharon?.Serial,
                FullPortCount = _mainCharon?.FullPortCount ?? 0,
                OwnPortCount = _mainCharon?.OwnPortCount ?? 0,
                Children = _mainCharon?.GetChildrenDto(),
                OtdrAddress = new NetAddress(otdrIp, otdrPort),
                Version = _version,
                Version2 = _versionIitOtdr,
                IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, false),
                AcceptableMeasParams = _treeOfAcceptableMeasParams,
            };
        }

    }
}