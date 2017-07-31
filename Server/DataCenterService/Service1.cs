using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using DataCenterCore;
using Iit.Fibertest.Utils35;

namespace DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IniFile _serviceIni;
        private readonly Logger35 _serviceLog;

        private DcManager _dcManager;

        public Service1()
        {
            InitializeComponent();
            _serviceIni = new IniFile();
            _serviceIni.AssignFile("DcService.ini");
            var cultureString = _serviceIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _serviceLog = new Logger35();
            _serviceLog.AssignFile("DcService.log", cultureString);

            _serviceLog.EmptyLine();
            _serviceLog.EmptyLine('-');
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            _dcManager = new DcManager();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
