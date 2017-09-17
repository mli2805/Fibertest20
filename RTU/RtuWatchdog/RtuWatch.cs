using System;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace RtuWatchdog
{
    public class RtuWatch
    {
        private readonly IniFile _watchIniFile;
        private readonly IMyLog _watchLog;

        public RtuWatch(IniFile watchIniFile, IMyLog watchLog)
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
                    _watchLog.AppendLine($"{rtuServiceName} is not running! Starting...");
                    sc.Start();
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    continue;
                }


                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }
}
