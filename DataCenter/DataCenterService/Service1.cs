using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public partial class Service1 : ServiceBase
    {
        public IniFile IniFile { get; }
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IParameterizer _serverParameterizer;
        private readonly EventStoreService _eventStoreService;
        private readonly IEventStoreInitializer _eventStoreInitializer;
        private readonly LastConnectionTimeChecker _lastConnectionTimeChecker;
        private readonly SignalRNudger _signalRNudger;
        private readonly VeexCompletedTestsFetcher _veexCompletedTestsFetcher;
        private readonly VeexCompletedTestsProcessorThread _veexCompletedTestsProcessorThread;
        private readonly WebApiChecker _webApiChecker;
        private readonly SmsSender _smsSender;
        private readonly SnmpTrapListener _snmpTrapListener;
        private readonly OutOfTurnProcessor _outOfTurnProcessor;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;
        private readonly WcfServiceForDesktopC2DBootstrapper _wcfServiceForDesktopC2DBootstrapper;
        private readonly WcfServiceForCommonC2DBootstrapper _wcfServiceForCommonC2DBootstrapper;
        private readonly WcfServiceForRtuBootstrapper _wcfServiceForRtuBootstrapper;
        private readonly WcfServiceForWebC2DBootstrapper _wcfServiceForWebC2DBootstrapper;
        private readonly IMsmqHandler _msmqHandler;

        public Service1(IniFile iniFile, IMyLog logFile, CurrentDatacenterParameters currentDatacenterParameters,
            IParameterizer serverParameterizer, EventStoreService eventStoreService, IEventStoreInitializer eventStoreInitializer,
            LastConnectionTimeChecker lastConnectionTimeChecker, SignalRNudger signalRNudger,
            VeexCompletedTestsFetcher veexCompletedTestsFetcher, VeexCompletedTestsProcessorThread veexCompletedTestsProcessorThread,
            WebApiChecker webApiChecker,
            SmsSender smsSender, SnmpTrapListener snmpTrapListener, OutOfTurnProcessor outOfTurnProcessor,
            IFtSignalRClient ftSignalRClient, MeasurementsForWebNotifier measurementsForWebNotifier,
            WcfServiceForDesktopC2DBootstrapper wcfServiceForDesktopC2DBootstrapper,
            WcfServiceForCommonC2DBootstrapper wcfServiceForCommonC2DBootstrapper,
            WcfServiceForRtuBootstrapper wcfServiceForRtuBootstrapper,
            WcfServiceForWebC2DBootstrapper wcfServiceForWebC2DBootstrapper,
            IMsmqHandler msmqHandler)
        {
            IniFile = iniFile;
            _logFile = logFile;
            _currentDatacenterParameters = currentDatacenterParameters;
            _serverParameterizer = serverParameterizer;
            _eventStoreService = eventStoreService;
            _eventStoreInitializer = eventStoreInitializer;
            _logFile.AssignFile("DataCenter.log");
            _lastConnectionTimeChecker = lastConnectionTimeChecker;
            _signalRNudger = signalRNudger;
            _veexCompletedTestsFetcher = veexCompletedTestsFetcher;
            _veexCompletedTestsProcessorThread = veexCompletedTestsProcessorThread;
            _webApiChecker = webApiChecker;
            _smsSender = smsSender;
            _snmpTrapListener = snmpTrapListener;
            _outOfTurnProcessor = outOfTurnProcessor;
            _ftSignalRClient = ftSignalRClient;
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
            Console.WriteLine(@"Service initialization done.");
        }

        private async void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Service initialization thread. Process {pid}, thread {tid}");

            var currentCulture = IniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            // used by user actions log composer
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);
            _logFile.AppendLine($"Current UI culture is {Thread.CurrentThread.CurrentUICulture}");

           
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            // for clean installation it would set "previous" version the same as current
            var previousStartOnVersion = IniFile.Read(IniSection.General, IniKey.Version, info.FileVersion);
            IniFile.Write(IniSection.Server, IniKey.PreviousStartOnVersion, previousStartOnVersion);
            _logFile.AppendLine($"Data-center version {info.FileVersion}");
            IniFile.Write(IniSection.General, IniKey.Version, info.FileVersion);

            _serverParameterizer.Init();
            _ftSignalRClient.Initialize();
            await InitializeEventStoreService();
            _lastConnectionTimeChecker.Start();

            if (_currentDatacenterParameters.WebApiBindingProtocol != "none")
            {
                _webApiChecker.Start();
                _signalRNudger.Start();
                _measurementsForWebNotifier.Start();
                _wcfServiceForWebC2DBootstrapper.Start();
            }

            _wcfServiceForCommonC2DBootstrapper.Start();
            _wcfServiceForDesktopC2DBootstrapper.Start();
            _wcfServiceForRtuBootstrapper.Start();
            _msmqHandler.Start();
            _veexCompletedTestsFetcher.Start();
            _veexCompletedTestsProcessorThread.Start();
            _smsSender.Start();
            _snmpTrapListener.Start();
            _outOfTurnProcessor.Start();
        }

        private async Task InitializeEventStoreService()
        {
            var resetDb = IniFile.Read(IniSection.MySql, IniKey.ResetDb, false);
            if (resetDb)
            {
                _logFile.AppendLine("ResetDb flag is TRUE! DB will be deleted...");
                using (var dbContext = new FtDbContext(_serverParameterizer.Options))
                {
                    dbContext.Database.EnsureDeleted();
                }
                _eventStoreInitializer.DropDatabase();
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

            using (var dbContext = new FtDbContext(_serverParameterizer.Options))
            {
                dbContext.Database.EnsureCreated();
                _serverParameterizer.LogSettings();
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