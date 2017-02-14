using Autofac;
using Caliburn.Micro;
using Serilog;

namespace Iit.Fibertest.TestBench
{
    public sealed class AutofacUi : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>();

            builder.RegisterType<ClientLogger>().SingleInstance();
        }
    }

    public class ClientLogger
    {
        public Serilog.Core.Logger Logger { get; set; }

        public ClientLogger()
        {
            Logger = new LoggerConfiguration().WriteTo.RollingFile("logs\\client.log").CreateLogger();
        }
    }
}