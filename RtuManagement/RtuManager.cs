using System;
using System.Diagnostics;
using System.Threading;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private Guid _id;
        private readonly Logger35 _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly Logger35 _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        public object WcfParameter { get; set; }

        private object _obj = new object();

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

        private bool _isMonitoringCancelled;

        public RtuManager(Logger35 serviceLog, IniFile serviceIni)
        {
            IsRtuInitialized = false;

            _serviceLog = serviceLog;
            _serviceIni = serviceIni;

            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _rtuLog = new Logger35();
            _rtuLog.AssignFile("RtuManager.log", cultureString);

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 40));
            _id = Guid.Parse(_rtuIni.Read(IniSection.DataCenter, IniKey.RtuGuid, Guid.Empty.ToString()));
        }

        public void Initialize()
        {
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');

            IsRtuInitialized = false;
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            bool isUserAskedInitialization = false;
            var rtu = WcfParameter as InitializeRtu;
            if (rtu != null)
            {
                isUserAskedInitialization = true;
                _rtuIni.Write(IniSection.DataCenter, IniKey.ServerIp, rtu.DataCenterIpAddress);
                _rtuIni.Write(IniSection.DataCenter, IniKey.RtuGuid, rtu.Id.ToString());
                WcfParameter = null;
            }
            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            if (InitializeRtuManager() != CharonOperationResult.Ok)
            {
                _rtuLog.AppendLine("Rtu Manager initialization failed.");
                if (isUserAskedInitialization)
                    SendInitializationConfirm(new RtuInitialized() {Id = rtu.Id, IsInitialized = false});
                return;
            }
            IsRtuInitialized = true;

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle();
            else
            {
                var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
                _otdrManager.DisconnectOtdr(otdrAddress);
                _rtuLog.AppendLine("Rtu is in MANUAL mode.");
            }

//            if (isUserAskedInitialization)
                SendInitializationConfirm(new RtuInitialized() {Id = _id, IsInitialized = true});
        }

        public void StartMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"RtuManager now in process {pid}, thread {tid}");

            if (IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in AUTOMATIC mode already.");
                return;
            }

            IsRtuInitialized = false;
            InitializeRtuManager();
            IsRtuInitialized = true;

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"Rtu is turned into AUTOMATIC mode. Process {pid}, thread {tid}");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            IsMonitoringOn = true;

            RunMonitoringCycle();

        }

        public void StopMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RtuManager now in process {pid}, thread {tid}");

            if (!IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in MANUAL mode already.");
                return;
            }

            _otdrManager.InterruptMeasurement();
            lock (_obj)
            {
                _isMonitoringCancelled = true;
            }

            /*
                        _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                        Thread.Sleep(TimeSpan.FromMilliseconds(2000));
                        var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
                        _otdrManager.DisconnectOtdr(otdrAddress);
                        IsMonitoringOn = false;
                        _isMonitoringCancelled = false;
                        _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
              */
        }

        public void MeasurementOutOfTurn()
        {
            var wasMonitoringOn = IsMonitoringOn;
            if (wasMonitoringOn)
            {
                StopMonitoring();
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }

            // TODO measurement

            if (wasMonitoringOn)
                StartMonitoring();
        }


    }
}