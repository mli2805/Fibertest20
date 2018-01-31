using System.ServiceProcess;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
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

            var logFile = new LogFile(iniFile);
            builder.RegisterInstance<IMyLog>(logFile);

            builder.RegisterType<MySqlEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();


            builder.RegisterType<WriteModel>().SingleInstance();
            builder.RegisterType<Aggregate>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();
            builder.RegisterType<MeasurementFactory>().SingleInstance();
            builder.RegisterType<MeasurementsRepository>().SingleInstance();
            builder.RegisterType<NetworkEventsRepository>().SingleInstance();
            builder.RegisterType<BopNetworkEventsRepository>().SingleInstance();
            builder.RegisterType<ClientStationsRepository>().SingleInstance();
            builder.RegisterType<RtuStationsRepository>().SingleInstance();
            builder.RegisterType<BaseRefsRepository>().SingleInstance();
            builder.RegisterType<BaseRefsBusinessToRepositoryIntermediary>().SingleInstance();

            builder.RegisterType<MonitoringResultsRepository>().SingleInstance();
            builder.RegisterType<GraphPostProcessingRepository>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();

            builder.RegisterType<D2CWcfManager>().SingleInstance();
            builder.RegisterType<LastConnectionTimeChecker>().SingleInstance();

            builder.RegisterType<D2RWcfManager>().As<ID2RWcfManager>().SingleInstance();

            builder.RegisterType<ServerSettings>().As<ISettings>().SingleInstance();

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