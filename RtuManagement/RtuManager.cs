using System;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private readonly Logger35 _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly Logger35 _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        private object _obj = new object();

        private readonly object _isMonitoringOnLocker = new object();
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

        private bool _isMonitoringCancelled;
        private bool _isMonitoringOn;

        public RtuManager(Logger35 serviceLog, IniFile serviceIni)
        {
            _serviceLog = serviceLog;
            _serviceIni = serviceIni;

            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _rtuLog = new Logger35();
            _rtuLog.AssignFile("RtuManager.log", cultureString);
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 40));
        }

        public void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            if (InitializeRtuManager() != CharonOperationResult.Ok)
            {
                _rtuLog.AppendLine("Rtu Manager initialization failed.");
                return;
            }

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle();
            else
            {
                var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
                _otdrManager.DisconnectOtdr(otdrAddress);
                _rtuLog.AppendLine("Rtu is in MANUAL mode.");
            }
        }

        public void StartMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
                _rtuLog.AppendLine($"RtuManager now in process {pid}, thread {tid}");

            if (IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in AUTOMATIC mode already.");
                return;
            }

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"Rtu is turned into AUTOMATIC mode. Process {pid}, thread {tid}");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            IsMonitoringOn = true;
            InitializeRtuManager();
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

            lock (_obj)
            {
                _otdrManager.InterruptMeasurement();
                _isMonitoringCancelled = true;
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
            }
            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
            lock (_obj)
            {
                IsMonitoringOn = false;
                _isMonitoringCancelled = false;
            }
        }

        public void MeasurementOutOfTurn()
        {

        }


    }
}