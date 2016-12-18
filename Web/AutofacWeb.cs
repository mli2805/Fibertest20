using Autofac;
using Autofac.Integration.WebApi;
using Serilog;

namespace Fibertest.Datacenter.Web
{
    internal class AutofacWeb : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(ThisAssembly);

            builder.RegisterInstance<ILogger>(new LoggerConfiguration()
                .WriteTo.Seq("http://local:5341")
                .CreateLogger());

            base.Load(builder);
        }
    }
}