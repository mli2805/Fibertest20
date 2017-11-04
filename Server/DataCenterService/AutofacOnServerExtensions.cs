using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterService
{
    public static class AutofacOnServerExtensions
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            var iniFile = new IniFile().AssignFile("DataCenter.ini");
            builder.RegisterInstance(iniFile);

//            builder.Register<IMyLog>(ctx =>
//                new LogFile(ctx.Resolve<IniFile>()).AssignFile("DataCenter.log"));

//            var logFile = new LogFile(iniFile).AssignFile("DataCenter.log");
//            filename will be assigned before first usage ( in Service1 ctor )
            var logFile = new LogFile(iniFile);
            builder.RegisterInstance<IMyLog>(logFile);



//            builder.RegisterType<SqliteEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();
            builder.RegisterType<MySqlEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();

            builder.RegisterType<MySqlContext>().As<IFibertestDbContext>().SingleInstance();

            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<Aggregate>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();
            builder.RegisterType<ClientRegistrationManager>().SingleInstance();
            builder.RegisterType<RtuRegistrationManager>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();
            builder.RegisterType<LastConnectionTimeChecker>().SingleInstance();

            builder.RegisterType<DcManager>().SingleInstance();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();
            builder.RegisterType<WcfServiceForClientBootstrapper>().SingleInstance();
            builder.RegisterType<WcfServiceForRtu>().As<IWcfServiceForRtu>().SingleInstance();
            builder.RegisterType<WcfServiceForRtuBootstrapper>().SingleInstance();
            builder.RegisterType<MsmqHandler>().SingleInstance();

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }
}