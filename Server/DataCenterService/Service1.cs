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
        private readonly BootstrapServiceForClient _bootstrapServiceForClient;

        public Service1(IniFile iniFile, IMyLog logFile, DcManager dcManager, BootstrapServiceForClient bootstrapServiceForClient)
        {
            IniFile = iniFile;
            LogFile = logFile;
            LogFile.AppendLine("I'm in Service1 ctor");
            _dcManager = dcManager;
            _bootstrapServiceForClient = bootstrapServiceForClient;
            InitializeComponent();

            LogFile.EmptyLine();
            LogFile.EmptyLine('-');
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            LogFile.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            _dcManager.Start();
            _bootstrapServiceForClient.Start();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            LogFile.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
