using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Utils35;
using RtuManagement;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RtuWcfService" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }

        private static Thread RtuManagerThread { get; set; }

        private readonly int _logLevel;
        private readonly RtuManager _rtuManager;

        public RtuWcfService()
        {
            _logLevel = ServiceIniFile.Read(IniSection.General, IniKey.LogLevel, 2);

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (_logLevel >= 2)
                ServiceLog?.AppendLine($"RtuWcfService started in process {pid}, thread {tid}");

            _rtuManager = new RtuManager(ServiceLog, ServiceIniFile);
            RtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
            RtuManagerThread.Start();
        }

        public void StartMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (_logLevel >= 2)
                ServiceLog.AppendLine($"RtuWcfService now in process {pid}, thread {tid}");

            ServiceLog.AppendLine("User asks to start monitoring");
            if (_rtuManager.IsMonitoringOn)
            {
                ServiceLog.AppendLine("Rtu is in AUTOMATIC mode already");
                return;
            }
            RtuManagerThread?.Abort();
            RtuManagerThread = new Thread(_rtuManager.StartMonitoring) { IsBackground = true };
            RtuManagerThread.Start();
        }

        public void StopMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (_logLevel >= 2)
                ServiceLog.AppendLine($"RtuWcfService now in process {pid}, thread {tid}");

            ServiceLog.AppendLine("User asks to stop monitoring");
            if (!_rtuManager.IsMonitoringOn)
            {
                ServiceLog.AppendLine("Rtu is in MANUAL mode already");
                return;
            }
            _rtuManager.StopMonitoring();
        }

    }
}
