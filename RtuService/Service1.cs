using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.Utils35;
using RtuManagement;
using RtuWcfServiceLibrary;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost MyServiceHost;

        private readonly IniFile _serviceIni;
        private readonly Logger35 _serviceLog;

        public Service1()
        {
            InitializeComponent();
            _serviceIni = new IniFile();
            _serviceIni.AssignFile("RtuService.ini");
            var cultureString = _serviceIni.Read(IniSection.General, IniKey.Culture, "ru-RU");
            
            _serviceLog = new Logger35();
            _serviceLog.AssignFile("RtuService.log", cultureString);

            _serviceLog.EmptyLine();
            _serviceLog.EmptyLine('-');
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();
            
            RtuWcfService.WcfIniFile = _serviceIni;
            RtuWcfService.WcfLogger35 = _serviceLog;
            MyServiceHost = new ServiceHost(typeof(RtuWcfService));
            MyServiceHost.Open();
        }

        protected override void OnStop()
        {
            if (MyServiceHost != null)
            {
                RtuWcfService.RtuManagerThread?.Abort();
                MyServiceHost.Close();
                MyServiceHost = null;
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}