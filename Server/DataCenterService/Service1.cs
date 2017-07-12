using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using System.Threading;
using DataCenterCore;
using Iit.Fibertest.Utils35;

namespace DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost ServiceForRtuHost;
        internal static ServiceHost ServiceForClientHost;

        private readonly IniFile _serviceIni;
        private readonly Logger35 _serviceLog;

        private readonly DcManager _dcManager;

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

            _dcManager = new DcManager();
        }

        protected override void OnStart(string[] args)
        {
            StartWcfServiceForRtu();
            StartWcfServiceForClient();
        }

        private void StartWcfServiceForRtu()
        {
            ServiceForRtuHost?.Close();

            WcfServiceForRtu.WcfServiceForRtu.ServiceIniFile = _serviceIni;
            WcfServiceForRtu.WcfServiceForRtu.ServiceLog = _serviceLog;
            WcfServiceForRtu.WcfServiceForRtu.DcManager = _dcManager;
            ServiceForRtuHost = new ServiceHost(typeof(WcfServiceForRtu.WcfServiceForRtu));
            try
            {
                ServiceForRtuHost.Open();
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                throw;
            }
        }

        private void StartWcfServiceForClient()
        {
            ServiceForClientHost?.Close();

            WcfServiceForClient.WcfServiceForClient.ServiceIniFile = _serviceIni;
            WcfServiceForClient.WcfServiceForClient.ServiceLog = _serviceLog;
            WcfServiceForClient.WcfServiceForClient.DcManager = _dcManager;
            ServiceForClientHost = new ServiceHost(typeof(WcfServiceForClient.WcfServiceForClient));
            try
            {
                ServiceForClientHost.Open();
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (ServiceForRtuHost != null)
            {
                ServiceForRtuHost.Close();
                ServiceForRtuHost = null;
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}
