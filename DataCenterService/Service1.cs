using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using System.Threading;
using D4C_WcfService;
using D4R_WcfService;
using Iit.Fibertest.Utils35;

namespace DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost D4RServiceHost;
        internal static ServiceHost D4CServiceHost;

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
            StartD4RService();
            StartD4CService();
        }

        private void StartD4RService()
        {
            D4RServiceHost?.Close();

            D4RWcfService.ServiceIniFile = _serviceIni;
            D4RWcfService.ServiceLog = _serviceLog;
            D4RServiceHost = new ServiceHost(typeof(D4RWcfService));
            try
            {
                D4RServiceHost.Open();
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                throw;
            }
        }

        private void StartD4CService()
        {
            D4CServiceHost?.Close();

            D4CWcfService.ServiceIniFile = _serviceIni;
            D4CWcfService.ServiceLog = _serviceLog;
            D4CServiceHost = new ServiceHost(typeof(D4CWcfService));
            try
            {
                D4CServiceHost.Open();
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (D4RServiceHost != null)
            {
                D4RServiceHost.Close();
                D4RServiceHost = null;
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
