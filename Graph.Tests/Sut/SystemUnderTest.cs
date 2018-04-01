using Autofac;
using Caliburn.Micro;
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
       // public IContainer Container { get; set; }
        public ILifetimeScope ClientContainer { get; set; }
        public ILifetimeScope ServerContainer { get; set; }

        public ReadModel ReadModel { get; }
        public GraphReadModel GraphReadModel { get; }
        public TreeOfRtuModel TreeOfRtuModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public IMyLog MyLogFile { get; set; }
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public ShellViewModel ShellVm { get; }

        public AccidentsExtractorFromSor AccidentsExtractorFromSor { get; }
        public MsmqHandler MsmqHandler { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;

        public const string NewTitleForTest = "New name for old equipment";
        public const EquipmentType NewTypeForTest = EquipmentType.Cross;
        public const int NewLeftCableReserve = 15;
        public const int NewRightCableReserve = 7;
        public const string NewCommentForTest = "New comment for old equipment";

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

            Poller = ClientContainer.Resolve<ClientPoller>();
            FakeWindowManager = (FakeWindowManager)ClientContainer.Resolve<IWindowManager>();
            MyLogFile = ClientContainer.Resolve<IMyLog>();
            ShellVm = (ShellViewModel)ClientContainer.Resolve<IShell>();
            ReadModel = (ReadModel)ClientContainer.Resolve<IModel>();
            GraphReadModel = ClientContainer.Resolve<GraphReadModel>();
            TreeOfRtuModel = ClientContainer.Resolve<TreeOfRtuModel>();
            TreeOfRtuViewModel = ClientContainer.Resolve<TreeOfRtuViewModel>();
            AccidentsExtractorFromSor = ClientContainer.Resolve<AccidentsExtractorFromSor>();
            MsmqHandler = ServerContainer.Resolve<MsmqHandler>();
            

            var ev = ServerContainer.Resolve<EventStoreService>();
            ev.Init();
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
            builder.RegisterType<Aggregate>().InstancePerLifetimeScope();
            builder.RegisterType<MeasurementFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EventsQueue>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();

            builder.RegisterType<TestSettings>().As<ISettings>().InstancePerLifetimeScope();

            builder.RegisterType<ClientStationsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RtuStationsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ClientToRtuTransmitter>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefRepairmanIntermediary>().InstancePerLifetimeScope();
            builder.RegisterType<SorFileRepository>().InstancePerLifetimeScope();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().InstancePerLifetimeScope();  // server !!!
            builder.RegisterType<D2CWcfManager>().InstancePerLifetimeScope();
            builder.RegisterType<MsmqHandler>().InstancePerLifetimeScope();

            builder.RegisterInstance<IMyLog>(new NullLog());

            builder.RegisterType<TestsDispatcherProvider>().As<IDispatcherProvider>().InstancePerLifetimeScope();
            var container = builder.Build();
            ClientContainer = container.BeginLifetimeScope();
            ServerContainer = container.BeginLifetimeScope();
        }
    }
}