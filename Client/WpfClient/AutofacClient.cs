﻿using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;
using Serilog;

namespace Iit.Fibertest.Client
{
    public class CurrentUser
    {
        public string UserName { get; set; }
        public Role Role { get; set; }
    }

    public sealed class AutofacClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GraphGpsCalculator>().SingleInstance();

            builder.RegisterType<CurrentUser>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

            builder.RegisterType<LocalDbManager>().As<ILocalDbManager>().SingleInstance();
            builder.RegisterType<ReflectogramManager>().SingleInstance();
            builder.RegisterType<TraceStateViewsManager>().SingleInstance();

            builder.RegisterType<OpticalEventsViewModel>();
            builder.RegisterType<OpticalEventsDoubleViewModel>().SingleInstance();
            builder.RegisterType<OpticalEventsProvider>().SingleInstance();

            builder.RegisterType<NetworkEventsViewModel>();
            builder.RegisterType<NetworkEventsDoubleViewModel>().SingleInstance();
            builder.RegisterType<NetworkEventsProvider>().SingleInstance();

            builder.RegisterType<BopNetworkEventsViewModel>();
            builder.RegisterType<BopNetworkEventsDoubleViewModel>().SingleInstance();
            builder.RegisterType<BopNetworkEventsProvider>().SingleInstance();

            builder.RegisterType<TabulatorViewModel>().SingleInstance();
            builder.RegisterType<CommonStatusBarViewModel>().SingleInstance();

            builder.RegisterType<RtuStateViewModel>();
            builder.RegisterType<TraceStateViewModel>();
            builder.RegisterType<TraceStatisticsViewModel>();

            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<OtauToAttachViewModel>().SingleInstance();
            builder.RegisterType<TraceToAttachViewModel>().SingleInstance();
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

            builder.RegisterType<BaseRefsChecker>().SingleInstance();
            builder.RegisterType<BaseRefDtoFactory>().SingleInstance();
            builder.RegisterType<BaseRefsAssignViewModel>();

            builder.RegisterType<RtuLeafActions>().SingleInstance();
            builder.RegisterType<RtuLeafActionsPermissions>().SingleInstance();
            builder.RegisterType<RtuLeafContextMenuProvider>().SingleInstance();

            builder.RegisterType<TraceLeafActions>().SingleInstance();
            builder.RegisterType<TraceLeafActionsPermissions>().SingleInstance();
            builder.RegisterType<TraceLeafContextMenuProvider>().SingleInstance();

            builder.RegisterType<PortLeafActions>().SingleInstance();
            builder.RegisterType<PortLeafContextMenuProvider>().SingleInstance();

            builder.RegisterType<CommonActions>().SingleInstance();

            builder.RegisterType<SoundManager>().SingleInstance();
            builder.RegisterType<RtuFilterViewModel>().SingleInstance();

            builder.RegisterType<RtuStateModelFactory>().SingleInstance();
            builder.RegisterType<RtuStateViewsManager>().SingleInstance();
            builder.RegisterType<TraceStateModelFactory>().SingleInstance();
            builder.RegisterType<TraceStateViewsManager>().SingleInstance();
            builder.RegisterType<TraceStatisticsViewsManager>().SingleInstance();

            builder.RegisterType<PortLeaf>();
            builder.RegisterType<OtauLeaf>();
            builder.RegisterType<RtuLeaf>();
            builder.RegisterType<TraceLeaf>();
            builder.RegisterType<TreeOfRtuModel>().SingleInstance();
            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<GraphReadModel>().SingleInstance();
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