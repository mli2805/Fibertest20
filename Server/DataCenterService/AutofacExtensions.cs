using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterService
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            var iniFile = new IniFile().AssignFile("DataCenter.ini");
            builder.RegisterInstance(iniFile);

//            builder.Register<IMyLog>(ctx =>
//                new LogFile(ctx.Resolve<LogFile>(iniFile)).AssignFile("DcService.log"));

            var logFile = new LogFile(iniFile).AssignFile("DataCenter.log");
            builder.RegisterInstance<IMyLog>(logFile);
            logFile.AppendLine("Log assigned");

            builder.RegisterType<DcManager>().SingleInstance();
            logFile.AppendLine("DcManager registered");

            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();
            logFile.AppendLine("WcfServiceForClient registered");

            builder.RegisterType<BootstrapServiceForClient>().SingleInstance();
            logFile.AppendLine("BootstrapServiceForClient registered");

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();
            logFile.AppendLine("Service1 registered");

            return builder;
        }
    }
}