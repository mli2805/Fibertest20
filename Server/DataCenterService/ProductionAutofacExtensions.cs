using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterService
{
    public static class ProductionAutofacExtensions
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            builder.RegisterInstance(new IniFile().AssignFile("DcService.ini"));

            builder.RegisterType<IMyLog>().As<IMyLog>();
            builder.RegisterType<Service1>().As<ServiceBase>();

            return builder;
        }
    }
}