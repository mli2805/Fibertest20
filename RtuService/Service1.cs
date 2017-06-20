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
        private RtuManager _rtuManager;

        private readonly IniFile _iniFile35;
        private readonly Logger35 _logger35;

        public Service1()
        {
            InitializeComponent();
            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("RtuService.ini");
            var cultureString = _iniFile35.Read(IniSection.General, IniKey.Culture, "ru-RU");
            
            _logger35 = new Logger35();
            _logger35.AssignFile("RtuService.log", cultureString);

            _logger35.EmptyLine();
            _logger35.EmptyLine('-');
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logger35.AppendLine($"Windows service started. Process {pid}, thread {tid}");
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();

            RtuWcfService.WcfIniFile = _iniFile35;
            RtuWcfService.WcfLogger35 = _logger35;
            MyServiceHost = new ServiceHost(typeof(RtuWcfService));
            MyServiceHost.Open();
             
            _rtuManager = new RtuManager();
            _rtuManager.Start();
//            Thread rtuManagerThread = new Thread(_rtuManager.Start);
//            rtuManagerThread.Start();
        }

        protected override void OnStop()
        {
            _rtuManager.Stop();

            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
                MyServiceHost = null;
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logger35.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}