using System.ServiceProcess;
using Autofac;
using DataCenterCore;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterService
{
    public static class ProductionAutofacExtensions
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            builder.RegisterInstance(new IniFile().AssignFile("DcService.ini"));
            builder.Register<IMyLog>(ctx =>
                new LogFile(ctx.Resolve<IniFile>()).AssignFile("DcService.log"));

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();
            builder.RegisterType<BootstrapServiceForClientHost>().SingleInstance();
            builder.RegisterType<DcManager>().SingleInstance();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();

            return builder;
        }
    }
}