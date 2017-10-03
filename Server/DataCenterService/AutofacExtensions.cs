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
            logFile.EmptyLine();
            logFile.EmptyLine('-');

            builder.RegisterInstance<IMyLog>(logFile);
            builder.RegisterType<DcManager>().SingleInstance();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();
            builder.RegisterType<WcfServiceForClientBootstrapper>().SingleInstance();
            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }
}