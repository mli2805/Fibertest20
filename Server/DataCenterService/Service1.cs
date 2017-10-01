using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using DataCenterCore;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        public IniFile ServiceIni { get; }
        public IMyLog ServiceLog { get; }

        private readonly DcManager _dcManager;

        public Service1(IniFile serviceIni, IMyLog serviceLog, DcManager dcManager)
        {
            _dcManager = dcManager;
            ServiceIni = serviceIni;
            ServiceLog = serviceLog;
            InitializeComponent();


            ServiceLog.EmptyLine();
            ServiceLog.EmptyLine('-');
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            ServiceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            _dcManager.Start();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            ServiceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
