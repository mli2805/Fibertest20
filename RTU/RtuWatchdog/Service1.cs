using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuWatchdog
{
    public partial class Service1 : ServiceBase
    {
        private readonly IniFile _watchdogIni;
        private readonly IMyLog _watchdogLog;
        public Service1()
        {
            InitializeComponent();
            _watchdogIni = new IniFile();
            _watchdogIni.AssignFile("RtuWatchdog.ini");

            _watchdogLog = new LogFile(_watchdogIni, 20000).AssignFile("RtuWatchdog.log");

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _watchdogLog.AppendLine($"RTU Watchdog service started. Process {pid}, thread {tid}");
            // takes version from RtuManagement.dll
            // mind maintain the same version for all assemblies
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _watchdogLog.AppendLine($"RTU Watchdog service version {info.FileVersion}"); 
        }

        protected override void OnStart(string[] args)
        {
            var rtuWatch = new RtuWatch(_watchdogIni, _watchdogLog);

            var watchThread = new Thread(rtuWatch.RunCycle) { IsBackground = true };
            watchThread.Start();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _watchdogLog.AppendLine($"RTU Watchdog service stopped. Process {pid}, thread {tid}");
        }
    }
}
