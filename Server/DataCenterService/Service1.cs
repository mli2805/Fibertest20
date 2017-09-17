using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using DataCenterCore;
using Dto;
using Iit.Fibertest.UtilsLib;

namespace DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;

        private DcManager _dcManager;

        public Service1()
        {
            InitializeComponent();
            _serviceIni = new IniFile();
            _serviceIni.AssignFile("DcService.ini");
            var cultureString = _serviceIni.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var logFileSizeLimit = _serviceIni.Read(IniSection.General, IniKey.LogFileSizeLimitKb, 0);

            _serviceLog = new LogFile();
            _serviceLog.AssignFile("DcService.log", logFileSizeLimit, cultureString);

            _serviceLog.EmptyLine();
            _serviceLog.EmptyLine('-');
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            var serverAddress = _serviceIni.ReadDoubleAddress((int) TcpPorts.ServerListenToRtu);
            _dcManager = new DcManager(serverAddress);
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
