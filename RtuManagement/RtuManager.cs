using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Restore, IniKey.MikrotikRebootTimeout, 40));
        }

        public void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            var initializationResult = InitializeMonitoring();
            while (initializationResult != CharonOperationResult.Ok)
                initializationResult = RecoverInitialization(initializationResult);

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

        private CharonOperationResult RecoverInitialization(CharonOperationResult error)
        {
            if (error == CharonOperationResult.AdditionalOtauError)
            {
                _rtuLog.AppendLine("Additional Otau recovering...");
                var bopIp = _mainCharon.Children.Values.First(c=>c.OwnPortCount == 0).NetAddress.Ip4Address;
                var damagedBop = _bopProblems.FirstOrDefault(b => b.Ip == bopIp);
                if (damagedBop != null)
                    damagedBop.RebootStarted = DateTime.Now;
                else
                    _bopProblems.Add(new BopProblem(bopIp));

                var mikrotik = new MikrotikInBop(_rtuLog, bopIp);
                if (mikrotik.Connect())
                    mikrotik.Reboot();

                _rtuLog.AppendLine("Next attempt to initialize");
                return InitializeMonitoring();
            }

            return CharonOperationResult.Ok;
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