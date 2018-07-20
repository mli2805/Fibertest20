using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public sealed class AutofacSc : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();

            builder.RegisterType<LogFile>().As<IMyLog>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();

            builder.RegisterType<SuperClientWcfService>().InstancePerLifetimeScope();
            builder.RegisterType<SuperClientWcfServiceHost>().InstancePerLifetimeScope();

            builder.RegisterType<FtServerList>().SingleInstance();
            builder.RegisterType<ChildStarter>().SingleInstance();

            builder.RegisterType<AddServerViewModel>();

        }
    }
}