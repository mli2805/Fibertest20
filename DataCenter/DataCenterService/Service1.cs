using System;
using System.Diagnostics;
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
        private readonly LastConnectionTimeChecker _lastConnectionTimeChecker;
        private readonly WcfServiceForClientBootstrapper _wcfServiceForClientBootstrapper;
        private readonly WcfServiceForRtuBootstrapper _wcfServiceForRtuBootstrapper;
        private readonly MsmqHandler _msmqHandler;

        public Service1(IniFile iniFile, IMyLog logFile, ISettings serverSettings,
            EventStoreService eventStoreService,
            LastConnectionTimeChecker lastConnectionTimeChecker,
            WcfServiceForClientBootstrapper wcfServiceForClientBootstrapper,
            WcfServiceForRtuBootstrapper wcfServiceForRtuBootstrapper,
            MsmqHandler msmqHandler)
        {
            IniFile = iniFile;
            _logFile = logFile;
            _serverSettings = serverSettings;
            _eventStoreService = eventStoreService;
            _logFile.AssignFile("DataCenter.log");
            _lastConnectionTimeChecker = lastConnectionTimeChecker;
            _wcfServiceForClientBootstrapper = wcfServiceForClientBootstrapper;
            _wcfServiceForRtuBootstrapper = wcfServiceForRtuBootstrapper;
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

        private void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Service initialization thread. Process {pid}, thread {tid}");

            _serverSettings.Init();
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

            using (var dbContext = new FtDbContext(_serverSettings.Options))
            {
                dbContext.Database.EnsureCreated();
                _serverSettings.LogSettings();
            }
            _eventStoreService.Init();
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

        // used for Debug as console application
        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }
    }
}