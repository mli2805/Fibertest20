using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;
using Serilog;

namespace Iit.Fibertest.Client
{
    public sealed class AutofacClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<MyWindowManager>().As<IMyWindowManager>().SingleInstance();

            builder.RegisterType<LocalDbManager>().As<ILocalDbManager>().SingleInstance();
            builder.RegisterType<ReflectogramManager>().SingleInstance();
            builder.RegisterType<TraceStateViewsManager>().SingleInstance();

            builder.RegisterType<OpticalEventsViewModel>();
            builder.RegisterType<OpticalEventsDoubleViewModel>().SingleInstance();
            builder.RegisterType<NetworkEventsViewModel>().SingleInstance();

            builder.RegisterType<TraceStateViewModel>();
            builder.RegisterType<TraceStatisticsViewModel>();

            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<UserListViewModel>();
            builder.RegisterType<ZonesViewModel>();
            builder.RegisterType<ObjectsToZonesViewModel>();
            builder.RegisterType<ZonesContentViewModel>();
            builder.RegisterType<ClientWcfService>().SingleInstance();
            builder.RegisterType<ClientWcfServiceHost>().As<IClientWcfServiceHost>().SingleInstance();

            var logger = new LoggerConfiguration()
                .WriteTo.Seq(@"http://localhost:5341").CreateLogger();
            builder.RegisterInstance<ILogger>(logger);
            logger.Information("");
            logger.Information(new string('-', 99));

            var iniFile = new IniFile();
            iniFile.AssignFile(@"Client.ini");
            builder.RegisterInstance(iniFile);
            var logFile = new LogFile(iniFile);
            builder.RegisterInstance<IMyLog>(logFile);

            var currentCulture =  iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            builder.RegisterType<Aggregate>().SingleInstance();
            builder.RegisterType<ReadModel>().SingleInstance();

            builder.RegisterType<RtuLeafActions>().SingleInstance();
            builder.RegisterType<RtuLeafActionsPermissions>().SingleInstance();
            builder.RegisterType<RtuLeafContextMenuProvider>().SingleInstance();

            builder.RegisterType<TraceLeafActions>().SingleInstance();
            builder.RegisterType<TraceLeafActionsPermissions>().SingleInstance();
            builder.RegisterType<TraceLeafContextMenuProvider>().SingleInstance();

            builder.RegisterType<SoundManager>().SingleInstance();

            builder.RegisterType<RtuStateVmFactory>().SingleInstance();
            builder.RegisterType<RtuStateViewsManager>().SingleInstance();
            builder.RegisterType<TraceStateVmFactory>().SingleInstance();
            builder.RegisterType<TraceStateViewsManager>().SingleInstance();
            builder.RegisterType<TraceStatisticsViewsManager>().SingleInstance();

            builder.RegisterType<TreeOfRtuModel>().SingleInstance();
            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<GraphReadModel>().SingleInstance();
            builder.RegisterType<PostOffice>().SingleInstance();
            builder.RegisterType<FreePorts>().SingleInstance();


            builder.RegisterType<C2DWcfManager>().AsSelf().As<IWcfServiceForClient>().SingleInstance();

            builder.RegisterType<ClientHeartbeat>().SingleInstance();

            builder.RegisterType<UiDispatcherProvider>().As<IDispatcherProvider>().SingleInstance();
            builder.Register(ioc => new ClientPoller(
                    ioc.Resolve<IWcfServiceForClient>(),
                    new List<object>
                    {
                        ioc.Resolve<ReadModel>(),
                        ioc.Resolve<TreeOfRtuModel>(),
                        ioc.Resolve<GraphReadModel>(),
                    },
                    ioc.Resolve<IDispatcherProvider>(),
                    ioc.Resolve<OpticalEventsDoubleViewModel>(),
                    ioc.Resolve<NetworkEventsViewModel>(),
                    logFile,
                    iniFile,
                    ioc.Resolve<ILocalDbManager>()
                    ))
                .SingleInstance();


            builder.RegisterType<LoginViewModel>();
            builder.RegisterType<ServerConnectViewModel>();
            builder.RegisterType<NetAddressTestViewModel>();
            builder.RegisterType<NetAddressInputViewModel>();
            builder.RegisterType<RtuInitializeViewModel>();

        }
    }
}