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

            builder.RegisterType<OpticalEventsViewModel>();
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

            builder.RegisterType<TraceLeafActions>().SingleInstance();
            builder.RegisterType<TraceLeafActionsPermissions>().SingleInstance();
            builder.RegisterType<TraceLeafContextMenuProvider>().SingleInstance();


            builder.RegisterType<TreeOfRtuModel>().SingleInstance();
            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<GraphReadModel>().SingleInstance();
            builder.RegisterType<PostOffice>().SingleInstance();
            builder.RegisterType<FreePorts>().SingleInstance();


            builder.RegisterType<C2DWcfManager>().AsSelf().As<IWcfServiceForClient>().SingleInstance();

            builder.RegisterType<ClientHeartbeat>().SingleInstance();

            builder.Register(ioc => new ClientPoller(
                    ioc.Resolve<IWcfServiceForClient>(),
                    new List<object>
                    {
                        ioc.Resolve<ReadModel>(),
                        ioc.Resolve<TreeOfRtuModel>(),
                        ioc.Resolve<GraphReadModel>()
                    },
                    ioc.Resolve<OpticalEventsViewModel>(),
                    logFile))
                .SingleInstance();


            builder.RegisterType<LoginViewModel>();
            builder.RegisterType<ServerConnectViewModel>();
            builder.RegisterType<NetAddressTestViewModel>();
            builder.RegisterType<NetAddressInputViewModel>();
            builder.RegisterType<RtuInitializeViewModel>();

        }
    }
}