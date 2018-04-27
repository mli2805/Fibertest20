﻿using Autofac;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

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
        public ShellViewModel ShellVm { get; set; }

        public AccidentsFromSorExtractor AccidentsFromSorExtractor { get; set; }
        public MsmqHandler MsmqHandler { get; }
        public WcfServiceForClient WcfService { get; set; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;

        public const string NewTitleForTest = "New name for old equipment";
        public const EquipmentType NewTypeForTest = EquipmentType.Cross;
        public const int NewLeftCableReserve = 15;
        public const int NewRightCableReserve = 7;
        public const string NewCommentForTest = "New comment for old equipment";

        public const string BaseTrace7 = @"..\..\Sut\BaseSorFiles\base-trace7.sor";
        public const string Base1625 = @"..\..\Sut\BaseSorFiles\base1625.sor";
        public const string Base1625Lm3 = @"..\..\Sut\BaseSorFiles\base1625-3lm.sor";
        public const string Base1550Lm2NoThresholds = @"..\..\Sut\BaseSorFiles\base1550-2lm-no-thresholds.sor";
        public const string Base1550Lm4YesThresholds = @"..\..\Sut\BaseSorFiles\base1550-4lm-3-thresholds.sor";
        public const string Base1550Lm2YesThresholds = @"..\..\Sut\BaseSorFiles\base1550-2lm-3-thresholds.sor";

        public const string Base1550Lm4RealplaceYesRough = @"..\..\Sut\BaseSorFiles\base1550-4lm-realplace-rough.sor";
        public const string Base1550Lm5FakeYesRough = @"..\..\Sut\BaseSorFiles\base1550-5lm-fake-rough.sor";

        public SystemUnderTest()
        {
            AutofacMess();

            var eventStoreService = ServerScope.Resolve<EventStoreService>();
            eventStoreService.Init();
            MsmqHandler = ServerScope.Resolve<MsmqHandler>();

            ResolveClientsPartsOnStart();

            LoginAsRoot();
        }

        private void LoginAsRoot()
        {
            var vm = ClientScope.Resolve<LoginViewModel>();
            vm.UserName = @"root";
            vm.Password = @"root";
            vm.Login();
            ShellVm.GetAlreadyStoredInCacheAndOnServerData().Wait();
            ReadModel.Users.Count.Should().Be(5);
        }

        private void ResolveClientsPartsOnStart()
        {
            ReadModel = ClientScope.Resolve<Model>();
            Poller = ClientScope.Resolve<ClientPoller>();

            FakeWindowManager = (FakeWindowManager) ClientScope.Resolve<IWindowManager>();
            MyLogFile = ClientScope.Resolve<IMyLog>();
            ShellVm = (ShellViewModel) ClientScope.Resolve<IShell>();
            GraphReadModel = ClientScope.Resolve<GraphReadModel>();
            TreeOfRtuModel = ClientScope.Resolve<TreeOfRtuModel>();
            TreeOfRtuViewModel = ClientScope.Resolve<TreeOfRtuViewModel>();
            AccidentsFromSorExtractor = ClientScope.Resolve<AccidentsFromSorExtractor>();

            WcfService = (WcfServiceForClient) ClientScope.Resolve<IWcfServiceForClient>();
        }

        public void RestartClient()
        {
            ClientScope = Container.BeginLifetimeScope(cfg =>
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceForClient>()));
            ResolveClientsPartsOnStart();

            ReadModel.Nodes.Count.Should().Be(0);
            GraphReadModel.Data.Nodes.Count.Should().Be(0);
        }

        private void AutofacMess()
        {
            var builder = new ContainerBuilder();

            // client's 
            builder.RegisterModule<AutofacClient>();

            // fakes
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<FakeD2RWcfManager>().As<ID2RWcfManager>().InstancePerLifetimeScope();
            builder.RegisterType<FakeLocalDbManager>().As<ILocalDbManager>().InstancePerLifetimeScope();
            builder.RegisterType<FakeClientWcfServiceHost>().As<IClientWcfServiceHost>();
            builder.RegisterType<FakeWaitCursor>().As<IWaitCursor>().InstancePerLifetimeScope();
            builder.RegisterType<FakeEventStoreInitializer>().As<IEventStoreInitializer>().InstancePerLifetimeScope();  // server!!!

            // server's
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().InstancePerLifetimeScope();  // server !!!

            builder.RegisterType<EventsQueue>().InstancePerLifetimeScope();
            builder.RegisterType<EventStoreService>().InstancePerLifetimeScope();

            builder.RegisterType<CommandAggregator>().InstancePerLifetimeScope();
            builder.RegisterType<MeasurementFactory>().InstancePerLifetimeScope();

            builder.RegisterType<TestSettings>().As<ISettings>().InstancePerLifetimeScope();

            builder.RegisterType<ClientStationsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RtuStationsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClientToRtuTransmitter>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefRepairmanIntermediary>().InstancePerLifetimeScope();
            builder.RegisterType<SorFileRepository>().InstancePerLifetimeScope();
            builder.RegisterType<D2CWcfManager>().InstancePerLifetimeScope();
            builder.RegisterType<Smtp>().InstancePerLifetimeScope();
            builder.RegisterType<MsmqHandler>().InstancePerLifetimeScope();

            builder.RegisterInstance<IMyLog>(new NullLog());

            builder.RegisterType<TestsDispatcherProvider>().As<IDispatcherProvider>().InstancePerLifetimeScope();
            Container = builder.Build();
            ServerScope = Container.BeginLifetimeScope();
            ClientScope = Container.BeginLifetimeScope(cfg =>
                cfg.RegisterInstance(ServerScope.Resolve<IWcfServiceForClient>()));
        }
    }
}