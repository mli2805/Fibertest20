using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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

//        private Dove _dove;
        private WoodPecker _woodPecker;

        public ConcurrentQueue<MoniResultOnDisk> QueueOfMoniResultsOnDisk { get; set; }
        private object WcfParameter { get; set; }

//        private bool _isMonitoringCancelled;
//        private readonly object _isMonitoringCancelledLocker = new object();

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

        public RtuManager(IMyLog serviceLog, IniFile serviceIni)
        {
            IsRtuInitialized = false;

            _serviceLog = serviceLog;
            _serviceIni = serviceIni;
            _serverAddresses = new DoubleAddressWithConnectionStats() { DoubleAddress = _serviceIni.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu), };

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
        }

        public RtuInitializedDto GetInitializationResult()
        {
            return new RtuInitializedDto()
            {
                RtuId = _id,
                IsInitialized = IsRtuInitialized,
                ErrorCode = IsRtuInitialized ? 0 : 99,
                Serial = _mainCharon.Serial,
                FullPortCount = _mainCharon.FullPortCount,
                OwnPortCount = _mainCharon.OwnPortCount,
                //                Children = _mainCharon.Children,
                Children = new Dictionary<int, OtauDto>(),
                OtdrAddress = _mainCharon.NetAddress,
                Version = _version,
            };
        }
    }
}