using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.RtuManagement;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuService
{
    public static class AutofacRtuManager
    {
        public static ContainerBuilder ForRtu(this ContainerBuilder builder)
        {
            var serviceIni = new IniFile().AssignFile("RtuService.ini");
            builder.RegisterInstance(serviceIni);

            var serviceLog = new LogFile(serviceIni, 20000);
            builder.RegisterInstance<IMyLog>(serviceLog);

            builder.RegisterType<BaseRefsSaver>().SingleInstance();

            builder.RegisterType<Heartbeat>().SingleInstance();
            builder.RegisterType<RtuManager>().SingleInstance();
            builder.RegisterType<RtuWcfOperationsPermissions>().SingleInstance();
            builder.RegisterType<WcfOtauOperator>().SingleInstance();
            builder.RegisterType<WcfMeasurementsOperator>().SingleInstance();
            builder.RegisterType<RtuWcfService>().As<IRtuWcfService>().SingleInstance();
            builder.RegisterType<RtuWcfServiceBootstrapper>().SingleInstance();

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }
}