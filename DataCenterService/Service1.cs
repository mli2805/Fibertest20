using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using System.Threading;
using D4R_WcfService;
using Iit.Fibertest.Utils35;

namespace DataCenterService
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
            _serviceIni.AssignFile("DcService.ini");
            var cultureString = _serviceIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _serviceLog = new Logger35();
            _serviceLog.AssignFile("DcService.log", cultureString);

            _serviceLog.EmptyLine();
            _serviceLog.EmptyLine('-');
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();

            D4RWcfService.ServiceIniFile = _serviceIni;
            D4RWcfService.ServiceLog = _serviceLog;
            MyServiceHost = new ServiceHost(typeof(D4RWcfService));
            try
            {
                MyServiceHost.Open();
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
                MyServiceHost = null;
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
