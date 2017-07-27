using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Dto;
using Iit.Fibertest.Utils35;
using RtuManagement;

namespace RtuWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }

        private static Thread RtuManagerThread { get; set; }

        private readonly RtuManager _rtuManager;

        private readonly object _lockObj = new object();

        public RtuWcfService()
        {
            if (ServiceIniFile == null)
            {
                ServiceIniFile = new IniFile();
                ServiceIniFile.AssignFile(@"WcfIniFile");
            }
            var logLevel = ServiceIniFile.Read(IniSection.General, IniKey.LogLevel, 2);

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (logLevel >= 2)
                ServiceLog?.AppendLine($"RtuWcfService started in process {pid}, thread {tid}");

            _rtuManager = new RtuManager(ServiceLog, ServiceIniFile);
            RtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
            RtuManagerThread.Start();
        }

        public bool IsRtuInitialized()
        {
            lock (_lockObj)
            {
                return _rtuManager.IsRtuInitialized;
            }
        }

        public bool Initialize(InitializeRtuDto rtu)
        {
            lock (_lockObj)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    ServiceLog.AppendLine("User demands initialization - Ignored - RTU is busy");
                    return false;
                }

                // can't just run _rtuManager.Initialize because it blocks Wcf thread
                RtuManagerThread?.Abort();
                _rtuManager.WcfParameter = rtu;
                RtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
                RtuManagerThread.Start();
                ServiceLog.AppendLine("User demands initialization - OK");
                return true;
            }
        }

        public void StartMonitoring()
        {
            lock (_lockObj)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    ServiceLog.AppendLine("User starts monitoring - Ignored - RTU is busy");
                    return;
                }
                if (!_rtuManager.IsMonitoringOn)
                {
                    // can't just run _rtuManager.StartMonitoring because it blocks Wcf thread
                    RtuManagerThread?.Abort();
                    RtuManagerThread = new Thread(_rtuManager.StartMonitoring) {IsBackground = true};
                    RtuManagerThread.Start();
                    ServiceLog.AppendLine("User starts monitoring - OK");
                }
                else
                    ServiceLog.AppendLine("User starts monitoring - Ignored - AUTOMATIC mode already");
            }
        }

        public void StopMonitoring()
        {
            lock (_lockObj)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    ServiceLog.AppendLine("User stops monitoring - Ignored - RTU is busy");
                    return;
                }
                if (_rtuManager.IsMonitoringOn)
                {
                    _rtuManager.StopMonitoring();
                    ServiceLog.AppendLine("User stops monitoring received");
                }
                else
                {
                    ServiceLog.AppendLine("User stops monitoring - Ignored - MANUAL mode already");
                }
            }
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            lock (_lockObj)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    ServiceLog.AppendLine("Monitoring settings received - Ignored - RTU is busy");
                    return false;
                }

                _rtuManager.ChangeSettings(settings);
                ServiceLog.AppendLine("Monitoring settings received");
                return true;
            }
        }

        public bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            lock (_lockObj)
            {
                if (_rtuManager.IsMonitoringOn)
                {
                    ServiceLog.AppendLine("User sent base ref - Ignored - RTU is busy");
                    return false;
                }

                _rtuManager.SaveBaseRefs(baseRef);

                ServiceLog.AppendLine("Base refs received");
                return true;
            }
        }

        public bool ToggleToPort(OtauPortDto port)
        {
            lock (_lockObj)
            {
                if (!_rtuManager.IsRtuInitialized || _rtuManager.IsMonitoringOn)
                {
                    ServiceLog.AppendLine("User demands port toggle - Ignored - RTU is busy");
                    return false;
                }
                return _rtuManager.ToggleToPort(port);
            }
        }
    }
}