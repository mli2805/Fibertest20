using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.SuperClient
{
    public sealed class AutofacSuperClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<D2CWcfManager>().SingleInstance();
            builder.RegisterType<DesktopC2DWcfManager>().SingleInstance();

            builder.RegisterType<LogFile>().As<IMyLog>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<WaitCursor>().As<IWaitCursor>();

            builder.RegisterType<SuperClientWcfService>().InstancePerLifetimeScope();
            builder.RegisterType<SuperClientWcfServiceHost>().InstancePerLifetimeScope();

            builder.RegisterType<ServersViewModel>().SingleInstance();
            builder.RegisterType<GasketViewModel>().SingleInstance();
            builder.RegisterType<FtServerList>().SingleInstance();
            builder.RegisterType<ChildStarter>().SingleInstance();

            builder.RegisterType<AddServerViewModel>();

        }
    }
}