using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using RtuManagement;
using RtuWcfServiceLibrary;
using WcfConnections;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;
        private RtuManager _rtuManager;
        private Thread _rtuManagerThread;

        public Service1()
        {
            InitializeComponent();
            _serviceIni = new IniFile();
            _serviceIni.AssignFile("RtuService.ini");

            _serviceLog = new LogFile(_serviceIni).AssignFile("RtuService.log");
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service started. Process {pid}, thread {tid}");



            StartWcfListener();

            _rtuManager = new RtuManager(_serviceLog, _serviceIni);
            _rtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
            _rtuManagerThread.Start();
        }

        protected override void OnStop()
        {
            _rtuManagerThread?.Abort();
            _myServiceHost?.Close();

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _serviceLog.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");

            // works very fast but trigger a window with swearing - demands one more click to close it
            // Environment.FailFast("Fast termination of service.");
        }

        private static ServiceHost _myServiceHost;
        private void StartWcfListener()
        {
            _myServiceHost?.Close();

            RtuWcfService.ServiceIniFile = _serviceIni;
            RtuWcfService.ServiceLog = _serviceLog;
            RtuWcfService.MessageReceived += RtuWcfService_MessageReceived;

            try
            {
                _myServiceHost = new ServiceHost(typeof(RtuWcfService));
                _myServiceHost.AddServiceEndpoint(typeof(IRtuWcfService),
                    WcfFactory.CreateDefaultNetTcpBinding(_serviceIni),
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.RtuListenTo, @"RtuWcfService"));

                _myServiceHost.Open();
                _serviceLog.AppendLine("RTU is listening to DataCenter now.");
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                var enabled = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemEnabled, false);
                if (enabled)
                {
                    var delay = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemDelay, 60);
                    _serviceLog.AppendLine("Recovery procedure: Reboot system.");
                    RestoreFunctions.RebootSystem(_serviceLog, delay);
                    Thread.Sleep(TimeSpan.FromSeconds(delay + 5));
                }
                else
                    throw;
            }

        }

        private void RtuWcfService_MessageReceived(object msg)
        {
            ProcessMessage(msg);
        }

        private void ProcessMessage(object msg)
        {
            var dto2 = msg as InitializeRtuDto;
            if (dto2 != null)
            {
                _rtuManagerThread?.Abort();

                _rtuManagerThread = new Thread(_rtuManager.Initialize) { IsBackground = true };
                _rtuManagerThread.Start(dto2);
                _serviceLog.AppendLine("User demands initialization - OK");
                return;
            }

            var dto3 = msg as AssignBaseRefDto;
            if (dto3 != null)
            {
                if (!_rtuManager.IsMonitoringOn)
                {
                    var thread = new Thread(_rtuManager.AssignBaseRefs);
                    thread.Start(dto3);
                    _serviceLog.AppendLine("Base refs received");
                }
                else
                    _serviceLog.AppendLine("User sent base ref - Ignored - RTU is busy");
                return;
            }

            var dto4 = msg as ApplyMonitoringSettingsDto;
            if (dto4 != null)
            {
                if (_rtuManager.IsRtuInitialized)
                {
                    _rtuManager.ChangeSettings(dto4);
                    _serviceLog.AppendLine("Monitoring settings received");
                }
                else
                    _serviceLog.AppendLine("Monitoring settings received - Ignored - RTU is busy");
                return;
            }

            var dto5 = msg as StartMonitoringDto;
            if (dto5 != null)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    _serviceLog.AppendLine("User starts monitoring - Ignored - RTU is busy");
                    return;
                }

                if (!_rtuManager.IsMonitoringOn)
                {
                    // can't just run _rtuManager.StartMonitoring because it blocks Wcf thread
                    _rtuManagerThread?.Abort();
                    _rtuManagerThread = new Thread(_rtuManager.StartMonitoring) { IsBackground = true };
                    _rtuManagerThread.Start();
                    _serviceLog.AppendLine("User starts monitoring - OK");
                }
                else
                    _serviceLog.AppendLine("User starts monitoring - Ignored - AUTOMATIC mode already");
                return;
            }

            var dto6 = msg as StopMonitoringDto;
            if (dto6 != null)
            {
                if (!_rtuManager.IsRtuInitialized)
                {
                    _serviceLog.AppendLine("User stops monitoring - Ignored - RTU is busy");
                    return;
                }
                if (_rtuManager.IsMonitoringOn)
                {
                    _rtuManager.StopMonitoring();
                    _serviceLog.AppendLine("User stops monitoring received");
                }
                else
                    _serviceLog.AppendLine("User stops monitoring - Ignored - MANUAL mode already");
                return;
            }

            var dto7 = msg as OtauPortDto;
            if (dto7 != null)
            {
                if (!_rtuManager.IsRtuInitialized || _rtuManager.IsMonitoringOn)
                    _serviceLog.AppendLine("User demands port toggle - Ignored - RTU is busy");
                else
                    _rtuManager.ToggleToPort(dto7);
                return;
            }

            var dtoWd = msg as LastSuccessfullMeasTimeDto;
            if (dtoWd != null)
            {
                _serviceLog.AppendLine("WatchDog cares");
            }

            _serviceLog.AppendLine("Message of unknown type was received. (Update ServiceReference)");
        }
    }
}