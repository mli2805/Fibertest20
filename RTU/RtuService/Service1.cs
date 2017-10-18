using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.RtuManagement;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IMyLog _serviceLog;
        private readonly RtuManager _rtuManager;
        private readonly RtuWcfServiceBootstrapper _rtuWcfServiceBootstrapper;
        private Thread _rtuManagerThread;

        public Service1(IMyLog serviceLog, RtuManager rtuManager, RtuWcfServiceBootstrapper rtuWcfServiceBootstrapper)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
            _rtuWcfServiceBootstrapper = rtuWcfServiceBootstrapper;
            _serviceLog.AssignFile("RtuService.log");
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");


            _rtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
            _rtuManagerThread.Start();
//            _rtuManager.Initialize();

            _rtuWcfServiceBootstrapper.Start();
        }

        protected override void OnStop()
        {
            _rtuManagerThread?.Abort();

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");

            // works very fast but trigger a window with swearing - demands one more click to close it
            // Environment.FailFast("Fast termination of service.");
        }

       
    }
}