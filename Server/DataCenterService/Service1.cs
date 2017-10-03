using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        public IniFile IniFile { get; }
        private IMyLog LogFile { get; }

        private readonly DcManager _dcManager;
        private readonly WcfServiceForClientBootstrapper _wcfServiceForClientBootstrapper;

        public Service1(IniFile iniFile, IMyLog logFile, DcManager dcManager, WcfServiceForClientBootstrapper wcfServiceForClientBootstrapper)
        {
            IniFile = iniFile;
            LogFile = logFile;
            LogFile.AssignFile("DataCenter.log");
            _dcManager = dcManager;
            _wcfServiceForClientBootstrapper = wcfServiceForClientBootstrapper;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            LogFile.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            _dcManager.Start();
            _wcfServiceForClientBootstrapper.Start();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            LogFile.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
