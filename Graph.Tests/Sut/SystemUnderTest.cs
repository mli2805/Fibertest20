using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public IContainer Container { get; set; }

        public ReadModel ReadModel { get; }
        public IMyLog MyLogFile { get; set; }
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public ShellViewModel ShellVm { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;

        public const string NewTitleForTest = "New name for old equipment";
        public const EquipmentType NewTypeForTest = EquipmentType.Cross;
        public const int NewLeftCableReserve = 15;
        public const int NewRightCableReserve = 7;
        public const string NewCommentForTest = "New comment for old equipment";

        public const string Base1625 = @"..\..\Sut\SorFiles\base1625.sor";
        public const string Base1625Lm3 = @"..\..\Sut\SorFiles\base1625-3lm.sor";
        public const string Base1550Lm2NoThresholds = @"..\..\Sut\SorFiles\base1550-2lm-no-thresholds.sor";
        public const string Base1550Lm4YesThresholds = @"..\..\Sut\SorFiles\base1550-4lm-3-thresholds.sor";
        public const string Base1550Lm2YesThresholds = @"..\..\Sut\SorFiles\base1550-2lm-3-thresholds.sor";

        public SystemUnderTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacClient>();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<FakeD2RWcfManager>().As<ID2RWcfManager>().SingleInstance();
            builder.RegisterType<FakeLocalDbManager>().As<ILocalDbManager>().SingleInstance();
            builder.RegisterType<FakeClientWcfServiceHost>().As<IClientWcfServiceHost>();
            builder.RegisterType<FakeWaitCursor>().As<IWaitCursor>().SingleInstance();

            builder.RegisterType<FakeEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();

            builder.RegisterType<TestSettings>().As<ISettings>().SingleInstance();

            builder.RegisterType<ClientStationsRepository>().SingleInstance();
            builder.RegisterType<RtuStationsRepository>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();
            builder.RegisterType<BaseRefsRepository>().SingleInstance();
            builder.RegisterType<BaseRefsBusinessToRepositoryIntermediary>().SingleInstance();
            builder.RegisterType<MeasurementsRepository>().SingleInstance();
            builder.RegisterType<NetworkEventsRepository>().SingleInstance();
            builder.RegisterType<BopNetworkEventsRepository>().SingleInstance();
            builder.RegisterType<GraphPostProcessingRepository>().SingleInstance();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();

            builder.RegisterInstance<IMyLog>(new NullLog());

            builder.RegisterType<TestsDispatcherProvider>().As<IDispatcherProvider>().SingleInstance();

            Container = builder.Build();

            Poller = Container.Resolve<ClientPoller>();
            FakeWindowManager = (FakeWindowManager) Container.Resolve<IWindowManager>();
            MyLogFile = Container.Resolve<IMyLog>();
            ShellVm = (ShellViewModel) Container.Resolve<IShell>();
            ReadModel = ShellVm.ReadModel;

            var ev = Container.Resolve<EventStoreService>();
            ev.Init();
        }
    }
}