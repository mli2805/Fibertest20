using System;
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
        private readonly Heartbeat _heartbeat;
        private Thread _rtuManagerThread;

        public Service1(IMyLog serviceLog, RtuManager rtuManager, RtuWcfServiceBootstrapper rtuWcfServiceBootstrapper, Heartbeat heartbeat)
        {
            _serviceLog = serviceLog;
            _rtuManager = rtuManager;
            _rtuWcfServiceBootstrapper = rtuWcfServiceBootstrapper;
            _heartbeat = heartbeat;
            _serviceLog.AssignFile("RtuService.log");
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            var upTime = Utils.GetUpTimeInSeconds();
            _serviceLog.AppendLine($"Windows up time is {upTime} seconds");
            if (upTime < 300)
            {
                _serviceLog.AppendLine("Additional pause after RTU restart is 20 sec");
                Thread.Sleep(20000);
            }

            _rtuManagerThread = new Thread(_rtuManager.OnServiceStart) { IsBackground = true };
            _rtuManagerThread.Start();

            _rtuWcfServiceBootstrapper.Start();

            var heartbeatThread = new Thread(_heartbeat.Start) {IsBackground = true};
            heartbeatThread.Start();
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

        // used for Debug as console application
        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }
    }
}