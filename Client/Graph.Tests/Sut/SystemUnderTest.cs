using System.Linq;
using Autofac;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public IContainer Container { get; set; }
        public ILifetimeScope ClientScope { get; set; }
        public ILifetimeScope ServerScope { get; set; }

        public Model ReadModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }
        public TreeOfRtuModel TreeOfRtuModel { get; set; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public IMyLog MyLogFile { get; set; }
        public ClientPoller Poller { get; set; }
        public FakeWindowManager FakeWindowManager { get; set; }
        public FakeD2RWcfManager FakeD2RWcfManager { get; set; }
        public ShellViewModel ShellVm { get; set; }
        public CurrentlyHiddenRtu CurrentlyHiddenRtu { get; set; }
        public string ConnectionId { get; set; }

        public AccidentsFromSorExtractor AccidentsFromSorExtractor { get; set; }
        public MsmqMessagesProcessor MsmqMessagesProcessor { get; }
        public WcfServiceDesktopC2D WcfServiceDesktopC2D { get; set; }
        public WcfServiceCommonC2D WcfServiceCommonC2D { get; set; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;

        public VeexCompletedTestsFetcher VeexCompletedTestsFetcher { get; set; }

        public const string NewTitleForTest = "New name for old equipment";
        public const EquipmentType NewTypeForTest = EquipmentType.Cross;
        public const int NewLeftCableReserve = 15;
        public const int NewRightCableReserve = 7;
        public const string NewCommentForTest = "New comment for old equipment";

        public const string BasTrace7 = @"..\..\Sut\BaseSorFiles\base-trace7.sor";
        public const string Base1625 = @"..\..\Sut\BaseSorFiles\base1625.sor";
        public const string Base1625Lm3 = @"..\..\Sut\BaseSorFiles\base1625-3lm.sor";
        public const string Base1550Lm2NoThresholds = @"..\..\Sut\BaseSorFiles\base1550-2lm-no-thresholds.sor";
        public const string Base1550Lm4YesThresholds = @"..\..\Sut\BaseSorFiles\base1550-4lm-3-thresholds.sor";
        public const string Base1550Lm2YesThresholds = @"..\..\Sut\BaseSorFiles\base1550-2lm-3-thresholds.sor";

        public const string Base1550Lm4RealPlaceYesRough = @"..\..\Sut\BaseSorFiles\base1550-4lm-realplace-rough.sor";
        public const string Base1550Lm5FakeYesRough = @"..\..\Sut\BaseSorFiles\base1550-5lm-fake-rough.sor";

        public const string Fibertest20dev = @"..\..\Sut\LicenseFiles\Fibertest20dev.lic";
        public const string DevSecAdmin = @"..\..\Sut\LicenseFiles\DevSecAdmin.lic";

        public SystemUnderTest()
        {
            AutofacMess();

            var eventStoreService = ServerScope.Resolve<EventStoreService>();
            eventStoreService.Init().Wait();
            MsmqMessagesProcessor = ServerScope.Resolve<MsmqMessagesProcessor>();
            VeexCompletedTestsFetcher = ServerScope.Resolve<VeexCompletedTestsFetcher>();
            FakeD2RWcfManager = (FakeD2RWcfManager)ServerScope.Resolve<ID2RWcfManager>();
            FakeD2RWcfManager.SetFakeInitializationAnswer();
            var writeModel = ServerScope.Resolve<Model>();
            writeModel.Users.Count.ShouldBeEquivalentTo(7);
            writeModel.Licenses.Count.ShouldBeEquivalentTo(0);

            ResolveClientsPartsOnStart();
        }

        private void ResolveClientsPartsOnStart()
        {
            ReadModel = ClientScope.Resolve<Model>();
            Poller = ClientScope.Resolve<ClientPoller>();

            FakeWindowManager = (FakeWindowManager)ClientScope.Resolve<IWindowManager>();

            MyLogFile = ClientScope.Resolve<IMyLog>();
            ShellVm = (ShellViewModel)ClientScope.Resolve<IShell>();
            CurrentlyHiddenRtu = ClientScope.Resolve<CurrentlyHiddenRtu>();
            GraphReadModel = ClientScope.Resolve<GraphReadModel>();

            // var dispatcherProvider = ClientScope.Resolve<IDispatcherProvider>();
            // dispatcherProvider.GetDispatcher().Invoke(() => GraphReadModel.MainMap = new Map());

            TreeOfRtuModel = ClientScope.Resolve<TreeOfRtuModel>();
            TreeOfRtuViewModel = ClientScope.Resolve<TreeOfRtuViewModel>();
            AccidentsFromSorExtractor = ClientScope.Resolve<AccidentsFromSorExtractor>();

            WcfServiceDesktopC2D = (WcfServiceDesktopC2D)ClientScope.Resolve<IWcfServiceDesktopC2D>();
            WcfServiceCommonC2D = (WcfServiceCommonC2D)ClientScope.Resolve<IWcfServiceCommonC2D>();
        }

        // by default with DEMO license
        public SystemUnderTest LoginOnEmptyBaseAsRoot(string licenseFilename = "")
        {
            string username = "root";
            var vm = ClientScope.Resolve<LoginViewModel>();
            vm.UserName = username;
            vm.PasswordViewModel.Password = username;
            vm.ConnectionId = @"connectionId" + username;

            // NoLicenseAppliedView - выберите демо или из файла
            FakeWindowManager.RegisterHandler(model => this.NoLicenseHandler(model, licenseFilename));

            if (licenseFilename != "") // взять из файла
                // формы выбора файла нет мы передаем его имя и сразу читаем ????
                // почему-то это MessageBox нужен и для файла лицензии с привязкой и без
                FakeWindowManager.RegisterHandler(model => this.ManyLinesMessageBoxAnswer(Answer.Yes, model));

            if (licenseFilename == DevSecAdmin)
                // форма для ввода пароля безопасника
                FakeWindowManager.RegisterHandler(model => this.SecurityAdminPasswordHandler(model, "lAChr6zA"));

            // MessageBox лицензия применена успешно
            FakeWindowManager.RegisterHandler(model => this.ManyLinesMessageBoxAnswer(Answer.Yes, model));

            ReadModel.Users.Count.Should().Be(0);
            vm.Login();
            vm.IsRegistrationSuccessful.ShouldBeEquivalentTo(true);
            ReadModel.Users.Count.Should().Be(0);
            ReadModel.Licenses.Count.Should().Be(0);

            if (vm.IsRegistrationSuccessful)
            {
                FakeWindowManager.RegisterHandler(model => model is WaitViewModel);
                ShellVm.GetAlreadyStoredOnServerData().Wait();
                ReadModel.Users.Count.Should().Be(licenseFilename == DevSecAdmin ? 8 : 7);
            }
            return this;
        }

        // login again when license has been applied already
        public void LoginAs(string username = "root", string secAdminPassword = "")
        {
            var vm = ClientScope.Resolve<LoginViewModel>();
            vm.UserName = username;
            vm.PasswordViewModel.Password = username;
            vm.ConnectionId = @"connectionId" + username;

            var writeModel = ServerScope.Resolve<Model>();
            if (writeModel.Licenses.First().IsMachineKeyRequired)
                FakeWindowManager.RegisterHandler(model => this.SecurityAdminPasswordHandler(model, secAdminPassword));

            FakeWindowManager.RegisterHandler(model => this.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            if (secAdminPassword == "wrong_password")
                FakeWindowManager.RegisterHandler(model => this.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            vm.Login();
            if (vm.IsRegistrationSuccessful)
            {
                FakeWindowManager.RegisterHandler(model => model is WaitViewModel);
                ShellVm.GetAlreadyStoredOnServerData().Wait();
            }
        }

        public void LogoutAs(string username)
        {
            var commonC2D = ClientScope.Resolve<IWcfServiceCommonC2D>();
            var connectionId = @"connectionId" + username;
            var unused = commonC2D.UnregisterClientAsync(
                new UnRegisterClientDto() { ClientIp = @"1.2.3.4", Username = username, ConnectionId = connectionId }).Result;
            ClientScope = Container.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceCommonC2D>());
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceDesktopC2D>());
            });
            ResolveClientsPartsOnStart();

            ReadModel.Nodes.Count.Should().Be(0);
            GraphReadModel.Data.Nodes.Count.Should().Be(0);
        }

        private void AutofacMess()
        {
            var builder = new ContainerBuilder();

            // client's 
            builder.RegisterModule<AutofacClient>();

            var iniFile = new IniFile();
            iniFile.AssignFile(@"client.ini");
            iniFile.Write(IniSection.ClientLocalAddress, IniKey.Ip, @"1.2.3.4");
            builder.RegisterInstance(iniFile);

            var parameters = new CommandLineParameters();
            builder.RegisterInstance(parameters);

            // fakes
            builder.RegisterType<FakeVeexRtuModel>().InstancePerLifetimeScope();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<FakeD2RWcfManager>().As<ID2RWcfManager>().InstancePerLifetimeScope();
            builder.RegisterType<FakeHttpWrapper>().As<IHttpWrapper>().InstancePerLifetimeScope();
            builder.RegisterType<FakeClientWcfServiceHost>().As<IClientWcfServiceHost>();
            builder.RegisterType<FakeWaitCursor>().As<IWaitCursor>().InstancePerLifetimeScope();
            builder.RegisterType<FakeMachineKeyProvider>().As<IMachineKeyProvider>().InstancePerLifetimeScope();
            builder.RegisterType<FakeLicenseFileChooser>().As<ILicenseFileChooser>().InstancePerLifetimeScope();
            builder.RegisterType<FakeEventStoreInitializer>().As<IEventStoreInitializer>().InstancePerLifetimeScope();  // server!!!

            // server's
            builder.RegisterType<WcfServiceDesktopC2D>().As<IWcfServiceDesktopC2D>().InstancePerLifetimeScope();  // server !!!
            builder.RegisterType<WcfServiceCommonC2D>().As<IWcfServiceCommonC2D>().InstancePerLifetimeScope();
            builder.RegisterType<WcfServiceForRtu>().As<IWcfServiceForRtu>().InstancePerLifetimeScope();  // server !!!
            builder.RegisterType<WcfServiceWebC2D>().InstancePerLifetimeScope();  // server !!!
            builder.RegisterType<MeasurementsForWebNotifier>().InstancePerLifetimeScope();  // server !!!
            

            builder.RegisterType<EventsQueue>().InstancePerLifetimeScope();
            builder.RegisterType<EventStoreService>().InstancePerLifetimeScope();
            builder.RegisterType<EventLogComposer>().InstancePerLifetimeScope();
            builder.RegisterType<EventToLogLineParser>().InstancePerLifetimeScope();

            builder.RegisterType<CommandAggregator>().InstancePerLifetimeScope();
            builder.RegisterType<MeasurementFactory>().InstancePerLifetimeScope();

            builder.RegisterType<TestParameterizer>().As<IParameterizer>().InstancePerLifetimeScope();

            builder.RegisterType<ClientsCollection>().InstancePerLifetimeScope();
            builder.RegisterType<RtuStationsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<IntermediateLayer>().InstancePerLifetimeScope();
            builder.RegisterType<ClientToRtuTransmitter>().InstancePerLifetimeScope();
            builder.RegisterType<ClientToRtuVeexTransmitter>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefRepairmanIntermediary>().InstancePerLifetimeScope();
            builder.RegisterType<SorFileRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SnapshotRepository>().InstancePerLifetimeScope();
            builder.RegisterType<D2CWcfManager>().InstancePerLifetimeScope();
            builder.RegisterType<D2RtuVeexLayer1>().SingleInstance();
            builder.RegisterType<D2RtuVeexLayer2>().SingleInstance();
            builder.RegisterType<D2RtuVeexLayer3>().SingleInstance();
            builder.RegisterType<SmsSender>().InstancePerLifetimeScope();
            builder.RegisterType<SmsManager>().InstancePerLifetimeScope();
            builder.RegisterType<Smtp>().InstancePerLifetimeScope();
            builder.RegisterType<SnmpNotifier>().InstancePerLifetimeScope();
            builder.RegisterType<SnmpAgent>().InstancePerLifetimeScope();
            builder.RegisterType<MsmqMessagesProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<VeexCompletedTestProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<VeexCompletedTestsFetcher>().InstancePerLifetimeScope();
            builder.RegisterType<CommonBopProcessor>().InstancePerLifetimeScope();
            builder.RegisterType<GlobalState>().InstancePerLifetimeScope();
            builder.RegisterType<DiskSpaceProvider>().InstancePerLifetimeScope();
            builder.RegisterType<FakeFtSignalRClient>().As<IFtSignalRClient>().InstancePerLifetimeScope();

            builder.RegisterInstance<IMyLog>(new NullLog());

            builder.RegisterType<TestsDispatcherProvider>().As<IDispatcherProvider>().InstancePerLifetimeScope();
            Container = builder.Build();
            ServerScope = Container.BeginLifetimeScope();
            ClientScope = Container.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceDesktopC2D>());
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceCommonC2D>());
            });
        }
    }
}