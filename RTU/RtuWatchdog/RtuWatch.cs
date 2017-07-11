using System;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.Utils35;

namespace RtuWatchdog
{
    public class RtuWatch
    {
        private readonly IniFile _watchIniFile;
        private readonly Logger35 _watchLog;

        public RtuWatch(IniFile watchIniFile, Logger35 watchLog)
        {
            _watchIniFile = watchIniFile;
            _watchLog = watchLog;
        }

        public void RunCycle()
        {
            var rtuServiceName = _watchIniFile.Read(IniSection.Watchdog, IniKey.RtuServiceName, "FibertestRtuService");
            while (true)
            {
                var sc = new ServiceController(rtuServiceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    _watchLog.AppendLine($"{rtuServiceName} is stopped! Starting...");
                    sc.Start();
                    Thread.Sleep(TimeSpan.FromSeconds(7));
                }

                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }
}
