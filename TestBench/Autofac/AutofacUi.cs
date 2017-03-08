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

//            builder.RegisterInstance<ILogger>(
//                new LoggerConfiguration()
//                    .WriteTo.RollingFile("logs\\client.log")
//                    .CreateLogger());
            builder.RegisterInstance<ILogger>(
                new LoggerConfiguration()
                    .WriteTo.Seq(@"http://localhost:5341")
                    .CreateLogger());
        }
    }

  
}