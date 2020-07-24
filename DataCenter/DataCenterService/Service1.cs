using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IEventStoreInitializer _eventStoreInitializer;
        private readonly LastConnectionTimeChecker _lastConnectionTimeChecker;
        private readonly SmsSender _smsSender;
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;
        private readonly WcfServiceForDesktopC2DBootstrapper _wcfServiceForDesktopC2DBootstrapper;
        private readonly WcfServiceForCommonC2DBootstrapper _wcfServiceForCommonC2DBootstrapper;
        private readonly WcfServiceForRtuBootstrapper _wcfServiceForRtuBootstrapper;
        private readonly WcfServiceForWebC2DBootstrapper _wcfServiceForWebC2DBootstrapper;
        private readonly IMsmqHandler _msmqHandler;

        public Service1(IniFile iniFile, IMyLog logFile, ISettings serverSettings,
            EventStoreService eventStoreService, IEventStoreInitializer eventStoreInitializer,
            LastConnectionTimeChecker lastConnectionTimeChecker, SmsSender smsSender,
            MeasurementsForWebNotifier measurementsForWebNotifier,
            WcfServiceForDesktopC2DBootstrapper wcfServiceForDesktopC2DBootstrapper,
            WcfServiceForCommonC2DBootstrapper wcfServiceForCommonC2DBootstrapper,
            WcfServiceForRtuBootstrapper wcfServiceForRtuBootstrapper,
            WcfServiceForWebC2DBootstrapper wcfServiceForWebC2DBootstrapper,
            IMsmqHandler msmqHandler)
        {
            IniFile = iniFile;
            _logFile = logFile;
            _serverSettings = serverSettings;
            _eventStoreService = eventStoreService;
            _eventStoreInitializer = eventStoreInitializer;
            _logFile.AssignFile("DataCenter.log");
            _lastConnectionTimeChecker = lastConnectionTimeChecker;
            _smsSender = smsSender;
            _measurementsForWebNotifier = measurementsForWebNotifier;
            _wcfServiceForDesktopC2DBootstrapper = wcfServiceForDesktopC2DBootstrapper;
            _wcfServiceForCommonC2DBootstrapper = wcfServiceForCommonC2DBootstrapper;
            _wcfServiceForRtuBootstrapper = wcfServiceForRtuBootstrapper;
            _wcfServiceForWebC2DBootstrapper = wcfServiceForWebC2DBootstrapper;
            _msmqHandler = msmqHandler;
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Windows service started. Process {pid}, thread {tid}");

            await Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            Console.WriteLine(@"Service initialization started...");
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Service initialization thread. Process {pid}, thread {tid}");

            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _logFile.AppendLine($"Data-center version {info.FileVersion}");
            IniFile.Write(IniSection.General, IniKey.Version, info.FileVersion);
     
            _serverSettings.Init();
            await InitializeEventStoreService();
            _lastConnectionTimeChecker.Start();
            _measurementsForWebNotifier.Start();
            _wcfServiceForCommonC2DBootstrapper.Start();
            _wcfServiceForWebC2DBootstrapper.Start();
            _wcfServiceForDesktopC2DBootstrapper.Start();
            _wcfServiceForRtuBootstrapper.Start();
            _msmqHandler.Start();
            _smsSender.Start();
            Console.WriteLine(@"Service initialization done.");
        }

        private async Task InitializeEventStoreService()
        {
            var resetDb = IniFile.Read(IniSection.MySql, IniKey.ResetDb, false);
            if (resetDb)
            {
                _logFile.AppendLine("ResetDb flag is TRUE! DB will be deleted...");
                using (var dbContext = new FtDbContext(_serverSettings.Options))
                {
                    dbContext.Database.EnsureDeleted();
                }
                _eventStoreService.Delete();
                IniFile.Write(IniSection.MySql, IniKey.ResetDb, false);
                _logFile.AppendLine("Db deleted successfully.");
            }
            else
                _eventStoreService.StreamIdOriginal = _eventStoreInitializer.GetStreamIdIfExists();

            if (_eventStoreService.StreamIdOriginal != Guid.Empty)
                _logFile.AppendLine($"Found DB with StreamIdOriginal {_eventStoreService.StreamIdOriginal}");
            else
            {
                _eventStoreService.StreamIdOriginal = Guid.NewGuid();
                _logFile.AppendLine($"DB will be created with StreamIdOriginal {_eventStoreService.StreamIdOriginal}");
            }

            using (var dbContext = new FtDbContext(_serverSettings.Options))
            {
                dbContext.Database.EnsureCreated();
                _serverSettings.LogSettings();
            }
            await _eventStoreService.Init();
        }

        protected override void OnStop()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Windows service stopped. Process {pid}, thread {tid}");
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