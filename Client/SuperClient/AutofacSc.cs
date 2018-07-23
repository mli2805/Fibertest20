using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.SuperClient
{
    public sealed class AutofacSc : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<D2CWcfManager>().SingleInstance();

            builder.RegisterType<LogFile>().As<IMyLog>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

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