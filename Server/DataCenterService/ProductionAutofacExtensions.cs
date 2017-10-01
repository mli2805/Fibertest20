using System.ServiceProcess;
using Autofac;
using DataCenterCore;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public static class ProductionAutofacExtensions
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            builder.RegisterInstance(new IniFile().AssignFile("DcService.ini"));
            builder.Register<IMyLog>(ctx =>
                new LogFile(ctx.Resolve<IniFile>()).WithFile("DcService.log"));

            builder.RegisterType<Service1>().As<ServiceBase>();
            builder.RegisterType<DcManager>();

            return builder;
        }
    }
}