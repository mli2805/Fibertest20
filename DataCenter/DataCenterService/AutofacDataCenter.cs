﻿using System.Globalization;
using System.ServiceProcess;
using System.Threading;
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
    public static class AutofacDataCenter
    {
        public static ContainerBuilder WithProduction(this ContainerBuilder builder)
        {
            var iniFile = new IniFile().AssignFile("DataCenter.ini");
            builder.RegisterInstance(iniFile);
            var currentCulture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            var logFile = new LogFile(iniFile, 100000);
            builder.RegisterInstance<IMyLog>(logFile);

            builder.RegisterType<MySqlEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();
            builder.RegisterType<ClientsCollection>().SingleInstance();

            builder.RegisterType<AccidentPlaceLocator>().SingleInstance();
            builder.RegisterType<AccidentsOnTraceToModelApplier>().SingleInstance();
            builder.RegisterType<EquipmentEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<NodeEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<FiberEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<TraceEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<RtuEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<UserEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<ZoneEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<EchoEventsOnModelExecutor>().SingleInstance();
            builder.RegisterType<MeasurementEventOnModelExecutor>().SingleInstance();
            builder.RegisterType<EventsOnModelExecutor>().SingleInstance();

            builder.RegisterType<Model>().As<Model>().InstancePerLifetimeScope();

            builder.RegisterType<EventsQueue>().SingleInstance();
            builder.RegisterType<CommandAggregator>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();
            builder.RegisterType<MeasurementFactory>().SingleInstance();
            builder.RegisterType<RtuStationsRepository>().SingleInstance();
            builder.RegisterType<GraphGpsCalculator>().SingleInstance();
            builder.RegisterType<TraceModelBuilder>().SingleInstance();
            builder.RegisterType<BaseRefLandmarksTool>().SingleInstance();
            builder.RegisterType<BaseRefDtoFactory>();
            builder.RegisterType<BaseRefRepairmanIntermediary>().SingleInstance();

            builder.RegisterType<SorFileRepository>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();

            builder.RegisterType<D2CWcfManager>().SingleInstance();
            builder.RegisterType<LastConnectionTimeChecker>().SingleInstance();
            builder.RegisterType<SmsSender>().SingleInstance();

            builder.RegisterType<D2RWcfManager>().As<ID2RWcfManager>().SingleInstance();

            builder.RegisterType<ServerSettings>().As<ISettings>().SingleInstance();

            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();
            builder.RegisterType<WcfServiceForClientBootstrapper>().SingleInstance();
            builder.RegisterType<WcfServiceForRtu>().As<IWcfServiceForRtu>().SingleInstance();
            builder.RegisterType<WcfServiceForRtuBootstrapper>().SingleInstance();
            builder.RegisterType<SorDataParsingReporter>();
            builder.RegisterType<AccidentsFromSorExtractor>();
            builder.RegisterType<SmsManager>().SingleInstance();
            builder.RegisterType<Smtp>().SingleInstance();
            builder.RegisterType<MsmqHandler>().SingleInstance();

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }

}