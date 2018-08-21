using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace KadastrLoader
{
    public sealed class AutofacKadastr : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KadastrLoaderViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<LogFile>().As<IMyLog>().InstancePerLifetimeScope();
        }
    }
}
