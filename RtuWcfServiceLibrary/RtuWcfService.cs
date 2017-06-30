﻿using System.Diagnostics;
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
            RtuManagerThread = new Thread(_rtuManager.Initialize) {IsBackground = true};
            RtuManagerThread.Start();
        }

        public string ShakeHandsWithWatchdog(string hello)
        {
            return hello;
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
                    ServiceLog.AppendLine("User stops monitoring - OK");
                }
                else
                {
                    ServiceLog.AppendLine("User stops monitoring - Ignored - MANUAL mode already");
                }
            }
        }
    }
}