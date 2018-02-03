using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        public IniFile IniFile { get; }
        private readonly IMyLog _logFile;
        private readonly ISettings _serverSettings;
        private readonly EventStoreService _eventStoreService;
        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly LastConnectionTimeChecker _lastConnectionTimeChecker;
        private readonly WcfServiceForClientBootstrapper _wcfServiceForClientBootstrapper;
        private readonly WcfServiceForRtuBootstrapper _wcfServiceForRtuBootstrapper;
        private readonly MsmqHandler _msmqHandler;

        public Service1(IniFile iniFile, IMyLog logFile, ISettings serverSettings,
            EventStoreService eventStoreService, 
            ClientStationsRepository clientStationsRepository,
            LastConnectionTimeChecker lastConnectionTimeChecker,
            WcfServiceForClientBootstrapper wcfServiceForClientBootstrapper,
            WcfServiceForRtuBootstrapper wcfServiceForRtuBootstrapper,
            MsmqHandler msmqHandler)
        {
            IniFile = iniFile;
            _logFile = logFile;
            _serverSettings = serverSettings;
            _eventStoreService = eventStoreService;
            _clientStationsRepository = clientStationsRepository;
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

            using (var dbContext = new FtDbContext(_serverSettings.Options))
            {
                    dbContext.Database.EnsureCreated();
            }
            _eventStoreService.Init();
            _clientStationsRepository.CleanClientStationsTable().Wait();
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