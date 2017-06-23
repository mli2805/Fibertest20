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
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        private object _obj = new object();
        public bool IsMonitoringOn { get; set; }
        private bool _isMonitoringCancelled;

        public RtuManager()
        {
            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _rtuLog = new Logger35();
            _rtuLog.AssignFile("RtuManager.log", cultureString);
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');
        }

        public void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);
            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (!InitializeMonitoring())
                // TODO recovering
                return;
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
            if (IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in AUTOMATIC mode already.");
                return;
            }

            _rtuLog.EmptyLine();
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"Rtu is turned into AUTOMATIC mode. Process {pid}, thread {tid}");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            InitializeMonitoring();
            RunMonitoringCycle();
        }

        public void StopMonitoring()
        {
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