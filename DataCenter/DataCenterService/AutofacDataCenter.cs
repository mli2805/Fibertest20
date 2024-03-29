﻿using System.Globalization;
using System.ServiceProcess;
using System.Threading;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

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

            var currentDatacenterParameters = new CurrentDatacenterParameters();
            builder.RegisterInstance(currentDatacenterParameters);
            builder.RegisterType<GlobalState>().SingleInstance();

            builder.RegisterType<MySqlEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();
            builder.RegisterType<ClientsCollection>().SingleInstance();

            builder.RegisterType<AccidentPlaceLocator>().SingleInstance();
            builder.RegisterType<AccidentLineModelFactory>().SingleInstance();
            builder.RegisterType<TraceStateReportProvider>().SingleInstance();

            builder.RegisterType<Model>().As<Model>().InstancePerLifetimeScope();

            builder.RegisterType<EventsQueue>().SingleInstance();
            builder.RegisterType<CommandAggregator>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();
            builder.RegisterType<MeasurementFactory>().SingleInstance();
            builder.RegisterType<SnapshotRepository>().SingleInstance();
            builder.RegisterType<RtuStationsRepository>().SingleInstance();
            builder.RegisterType<GraphGpsCalculator>().SingleInstance();
            builder.RegisterType<TraceModelBuilder>().SingleInstance();
            builder.RegisterType<BaseRefLandmarksTool>().SingleInstance();
            builder.RegisterType<LandmarksBaseParser>().SingleInstance();
            builder.RegisterType<LandmarksGraphParser>().SingleInstance();
            builder.RegisterType<BaseRefDtoFactory>();
            builder.RegisterType<BaseRefRepairmanIntermediary>().SingleInstance();
            builder.RegisterType<BaseRefsChecker2>().SingleInstance();

            builder.RegisterType<SorFileRepository>().SingleInstance();
            builder.RegisterType<IntermediateLayer>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();
            builder.RegisterType<ClientToRtuVeexTransmitter>().SingleInstance();

            builder.RegisterType<D2CWcfManager>().SingleInstance();
            builder.RegisterType<WebApiChecker>().SingleInstance();
            builder.RegisterType<LastConnectionTimeChecker>().SingleInstance();
            builder.RegisterType<SignalRNudger>().SingleInstance();
            builder.RegisterType<MeasurementsForWebNotifier>().SingleInstance();
            builder.RegisterType<VeexCompletedTestsFetcher>().SingleInstance();
            builder.RegisterType<VeexCompletedTestProcessor>().SingleInstance();
            builder.RegisterType<SmsSender>().SingleInstance();

            builder.RegisterType<D2RWcfManager>().As<ID2RWcfManager>().SingleInstance();


            builder.RegisterType<HttpExt>().SingleInstance();
            builder.RegisterType<D2RtuVeexLayer1>().SingleInstance();
            builder.RegisterType<D2RtuVeexLayer2>().SingleInstance();
            builder.RegisterType<D2RtuVeexLayer3>().SingleInstance();

            builder.RegisterType<ServerParameterizer>().As<IParameterizer>().SingleInstance();

            builder.RegisterType<WcfServiceDesktopC2D>().As<IWcfServiceDesktopC2D>().SingleInstance();
            builder.RegisterType<WcfServiceForDesktopC2DBootstrapper>().SingleInstance();
            builder.RegisterType<WcfServiceCommonC2D>().As<IWcfServiceCommonC2D>().SingleInstance();
            builder.RegisterType<WcfServiceForCommonC2DBootstrapper>().SingleInstance();
            builder.RegisterType<WcfServiceForRtu>().As<IWcfServiceForRtu>().SingleInstance();
            builder.RegisterType<WcfServiceForRtuBootstrapper>().SingleInstance();
            builder.RegisterType<WcfServiceWebC2D>().As<IWcfServiceWebC2D>().SingleInstance();
            builder.RegisterType<WcfServiceForWebC2DBootstrapper>().SingleInstance();
            builder.RegisterType<FtSignalRClient>().As<IFtSignalRClient>().SingleInstance();

            builder.RegisterType<SorDataParsingReporter>();
            builder.RegisterType<AccidentsFromSorExtractor>();
            builder.RegisterType<SmsManager>().SingleInstance();
            builder.RegisterType<Smtp>().SingleInstance();
            builder.RegisterType<SnmpNotifier>().SingleInstance();
            builder.RegisterType<SnmpAgent>().SingleInstance();
            builder.RegisterType<MsmqMessagesProcessor>().SingleInstance();
            builder.RegisterType<MsmqHandler>().As<IMsmqHandler>().SingleInstance();
            builder.RegisterType<DiskSpaceProvider>().SingleInstance();

            builder.RegisterType<OltTrapParser>().SingleInstance();
            builder.RegisterType<OltTrapExecutor>().SingleInstance();
            builder.RegisterType<TrapReceiver>().SingleInstance();

            builder.RegisterType<Service1>().As<ServiceBase>().SingleInstance();

            return builder;
        }
    }

}