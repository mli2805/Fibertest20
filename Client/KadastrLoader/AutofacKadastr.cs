using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace KadastrLoader
{
    public sealed class AutofacKadastr : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KadastrLoaderViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<LogFile>().As<IMyLog>().InstancePerLifetimeScope();
            builder.RegisterType<C2DWcfManager>().AsSelf().As<IWcfServiceForClient>().InstancePerLifetimeScope();


            builder.RegisterType<KadastrDbSettings>().SingleInstance();
            builder.RegisterType<KadastrDbProvider>().SingleInstance();


            builder.RegisterType<LoadedAlready>().SingleInstance();
            builder.RegisterType<KadastrFilesParser>().SingleInstance();
            builder.RegisterType<WellParser>().SingleInstance();
            builder.RegisterType<ChannelParser>().SingleInstance();
            builder.RegisterType<ConpointParser>().SingleInstance();
        }
    }
}
