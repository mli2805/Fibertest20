using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.Utils35;

namespace RtuWatchdog
{
    public partial class Service1 : ServiceBase
    {
        private readonly IniFile _watchdogIni;
        private readonly Logger35 _watchdogLog;
        public Service1()
        {
            InitializeComponent();
            _watchdogIni = new IniFile();
            _watchdogIni.AssignFile("Watchdog.ini");
            var cultureString = _watchdogIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _watchdogLog = new Logger35();
            _watchdogLog.AssignFile("Watchdog.log", cultureString);

            _watchdogLog.EmptyLine();
            _watchdogLog.EmptyLine('-');
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _watchdogLog.AppendLine($"Watchdog service started. Process {pid}, thread {tid}");
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
            _watchdogLog.AppendLine($"Watchdog service stopped. Process {pid}, thread {tid}");
        }
    }
}
