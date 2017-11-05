using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        public IniFile IniFile { get; }
        private readonly IMyLog _logFile;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly WriteModel _writeModel;
        private readonly RtuRegistrationManager _rtuRegistrationManager;
        private readonly LastConnectionTimeChecker _lastConnectionTimeChecker;
        private readonly WcfServiceForClientBootstrapper _wcfServiceForClientBootstrapper;
        private readonly WcfServiceForRtuBootstrapper _wcfServiceForRtuBootstrapper;
        private readonly MsmqHandler _msmqHandler;

        public Service1(IniFile iniFile, IMyLog logFile,
            EventStoreService eventStoreService, 
            ClientRegistrationManager clientRegistrationManager,
            WriteModel writeModel,
            RtuRegistrationManager rtuRegistrationManager,
            LastConnectionTimeChecker lastConnectionTimeChecker,
            WcfServiceForClientBootstrapper wcfServiceForClientBootstrapper,
            WcfServiceForRtuBootstrapper wcfServiceForRtuBootstrapper,
            MsmqHandler msmqHandler)
        {
            IniFile = iniFile;
            _logFile = logFile;
            _eventStoreService = eventStoreService;
            _clientRegistrationManager = clientRegistrationManager;
            _writeModel = writeModel;
            _rtuRegistrationManager = rtuRegistrationManager;
            _logFile.AssignFile("DataCenter.log");
            _lastConnectionTimeChecker = lastConnectionTimeChecker;
            _wcfServiceForClientBootstrapper = wcfServiceForClientBootstrapper;
            _wcfServiceForRtuBootstrapper = wcfServiceForRtuBootstrapper;
            _msmqHandler = msmqHandler;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            _eventStoreService.Init();
            _clientRegistrationManager.CleanClientStations().Wait();
            _rtuRegistrationManager.InitManager(_writeModel.GetInitializedRtuList()).Wait();
            _lastConnectionTimeChecker.Start();
            _wcfServiceForClientBootstrapper.Start();
            _wcfServiceForRtuBootstrapper.Start();
            _msmqHandler.Start();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
        }
    }
}