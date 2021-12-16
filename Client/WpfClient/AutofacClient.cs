using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public sealed class AutofacClient : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GraphGpsCalculator>().InstancePerLifetimeScope();

            builder.RegisterType<CurrentUser>().InstancePerLifetimeScope();
            builder.RegisterType<CurrentlyHiddenRtu>().InstancePerLifetimeScope();
            builder.RegisterType<CurrentGis>().InstancePerLifetimeScope();
            builder.RegisterType<CurrentDatacenterParameters>().InstancePerLifetimeScope();
            builder.RegisterType<SystemState>().InstancePerLifetimeScope();
            builder.RegisterType<WindowManager>().As<IWindowManager>().InstancePerLifetimeScope();
            builder.RegisterType<ConfigurationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<EventToLogLineParser>().InstancePerLifetimeScope();
            builder.RegisterType<EventLogComposer>().InstancePerLifetimeScope();
            builder.RegisterType<LogOperationsViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<EventLogViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<AboutViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<WaitViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseFromFileDecoder>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseFileChooser>().As<ILicenseFileChooser>().InstancePerLifetimeScope();

            builder.RegisterType<ReflectogramManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateViewsManager>().InstancePerLifetimeScope();

            builder.RegisterType<OpticalEventsViewModel>();
            builder.RegisterType<OpticalEventsDoubleViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<OpticalEventsExecutor>().InstancePerLifetimeScope();

            builder.RegisterType<NetworkEventsViewModel>();
            builder.RegisterType<NetworkEventsDoubleViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<BopNetworkEventsViewModel>();
            builder.RegisterType<BopNetworkEventsDoubleViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<TabulatorViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<CommonStatusBarViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<BopStateViewModel>();
            builder.RegisterType<RtuStateViewModel>();
            builder.RegisterType<TraceStateViewModel>();
            builder.RegisterType<BaseRefModelFactory>();
            builder.RegisterType<TraceStatisticsViewModel>();

            builder.RegisterType<LicenseCommandFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseSender>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<NoLicenseAppliedViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MainMenuViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<OtauToAttachViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<TraceToAttachViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<RtuUpdateViewModel>();
            builder.RegisterType<UserViewModel>();
            builder.RegisterType<UserListViewModel>();
            builder.RegisterType<ZoneViewModel>();
            builder.RegisterType<ZonesViewModel>();
            builder.RegisterType<ObjectsAsTreeToZonesViewModel>();
            builder.RegisterType<GisSettingsViewModel>();
            builder.RegisterType<SmtpSettingsViewModel>();
            builder.RegisterType<SmsSettingsViewModel>();
            builder.RegisterType<SnmpSettingsViewModel>();
            builder.RegisterType<DbOptimizationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewModel>();
            builder.RegisterType<WcfServiceInClient>().InstancePerLifetimeScope();
            builder.RegisterType<ClientWcfServiceHost>().As<IClientWcfServiceHost>().InstancePerLifetimeScope();

            builder.RegisterType<WaitCursor>().As<IWaitCursor>();

            builder.RegisterType<LogFile>().As<IMyLog>().InstancePerLifetimeScope();

            builder.RegisterType<Model>().InstancePerLifetimeScope();

            builder.RegisterType<BaseRefsChecker2>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefMessages>().InstancePerLifetimeScope();
            builder.RegisterType<TraceModelBuilder>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefLandmarksTool>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefMeasParamsChecker>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefLandmarksChecker>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefDtoFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BaseRefsAssignViewModel>();

            builder.RegisterType<RtuLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuLeafActionsPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuLeafContextMenuProvider>().InstancePerLifetimeScope();

            builder.RegisterType<TraceLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<TraceLeafActionsPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<TraceLeafContextMenuProvider>().InstancePerLifetimeScope();

            builder.RegisterType<PortLeafActions>().InstancePerLifetimeScope();
            builder.RegisterType<PortLeafContextMenuProvider>().InstancePerLifetimeScope();

            builder.RegisterType<CommonActions>().InstancePerLifetimeScope();

            builder.RegisterType<SoundManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuRemover>().InstancePerLifetimeScope();
            builder.RegisterType<RtuFilterViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<RtuStateModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BopStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuChannelViewModel>();
            builder.RegisterType<RtuChannelViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<RtuStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentLineModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStateViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceStatisticsViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<TraceInfoViewModel>();
            builder.RegisterType<StepChoiceViewModel>();
            builder.RegisterType<TraceStepByStepViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<PortLeaf>();
            builder.RegisterType<OtauLeaf>();
            builder.RegisterType<RtuLeaf>();
            builder.RegisterType<TraceLeaf>();

            builder.RegisterType<EchoEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<TraceEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<InitializeRtuEventOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<RtuEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<ZoneEventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EventsOnTreeExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<TreeOfRtuModel>().InstancePerLifetimeScope();
            builder.RegisterType<TreeOfRtuViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<GrmEquipmentRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmNodeRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmFiberWithNodesRequest>().InstancePerLifetimeScope();
            builder.RegisterType<GrmFiberRequests>().InstancePerLifetimeScope();
            builder.RegisterType<GrmRtuRequests>().InstancePerLifetimeScope();

            builder.RegisterType<SorDataParsingReporter>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentsFromSorExtractor>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentPlaceLocator>().InstancePerLifetimeScope();
            builder.RegisterType<NodeEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<FiberEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EquipmentEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<TraceEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<RtuEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<AccidentEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<ResponsibilityEventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<EventsOnGraphExecutor>().InstancePerLifetimeScope();
            builder.RegisterType<GraphReadModel>().InstancePerLifetimeScope();

            builder.RegisterType<FreePorts>().InstancePerLifetimeScope();
            builder.RegisterType<ChildrenViews>().InstancePerLifetimeScope();

            builder.RegisterType<TraceContentChoiceViewModel>();
            builder.RegisterType<EquipmentOfChoiceModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DesktopC2DWcfManager>().AsSelf().As<IWcfServiceDesktopC2D>().InstancePerLifetimeScope();
            builder.RegisterType<CommonC2DWcfManager>().AsSelf().As<IWcfServiceCommonC2D>().InstancePerLifetimeScope();
            builder.RegisterType<C2SWcfManager>().AsSelf().As<IWcfServiceInSuperClient>().InstancePerLifetimeScope();

            builder.RegisterType<UiDispatcherProvider>().As<IDispatcherProvider>().InstancePerLifetimeScope();
            builder.RegisterType<RenderingApplierToUi>();
            builder.RegisterType<OneRtuOrTraceRenderer>();
            builder.RegisterType<CurrentZoneRenderer>();
            builder.RegisterType<LessThanRootRenderer>();
            builder.RegisterType<RootRenderer>();
            builder.RegisterType<RenderingManager>();
            builder.RegisterType<EventArrivalNotifier>().InstancePerLifetimeScope();
            builder.RegisterType<Heartbeater>().InstancePerLifetimeScope();
            builder.RegisterType<ClientPoller>().InstancePerLifetimeScope();

            builder.RegisterType<AddEquipmentIntoNodeBuilder>();
            builder.RegisterType<GpsInputViewModel>();
            builder.RegisterType<GpsInputSmallViewModel>();
            builder.RegisterType<LandmarksGraphParser>();
            builder.RegisterType<LandmarksBaseParser>();
            builder.RegisterType<OneLandmarkViewModel>();
            builder.RegisterType<TraceChoiceViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<LandmarksViewModel>();
            builder.RegisterType<LandmarksViewsManager>().InstancePerLifetimeScope();
            builder.RegisterType<NodeUpdateViewModel>();
            builder.RegisterType<FiberUpdateViewModel>();
            builder.RegisterType<EquipmentInfoViewModel>();

            builder.RegisterType<MonitoringSettingsModelFactory>();
            builder.RegisterType<MonitoringSettingsViewModel>();

            builder.RegisterType<MachineKeyProvider>().As<IMachineKeyProvider>();
            builder.RegisterType<LoginViewModel>();
            builder.RegisterType<ServersConnectViewModel>();
            builder.RegisterType<SecurityAdminConfirmationViewModel>();
            builder.RegisterType<ServerConnectionLostViewModel>();
            builder.RegisterType<NetAddressTestViewModel>();
            builder.RegisterType<NetAddressInputViewModel>();
            builder.RegisterType<RtuInitializeModel>();
            builder.RegisterType<RtuInitializeViewModel>();
            builder.RegisterType<OnDemandMeasurement>().InstancePerLifetimeScope();
            builder.RegisterType<OtdrParametersThroughServerSetterViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ClientMeasurementViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<OutOfTurnPreciseMeasurementViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<NodeVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<CommonVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<NodeVmPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<NodeVmContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmActions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmPermissions>().InstancePerLifetimeScope();
            builder.RegisterType<RtuVmContextMenuProvider>().InstancePerLifetimeScope();
            builder.RegisterType<MapActions>().InstancePerLifetimeScope();
            builder.RegisterType<MapContextMenuProvider>().InstancePerLifetimeScope();

            builder.RegisterType<ComponentsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ComponentsReportViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ActualOpticalEventsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<AllOpticalEventsReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<OpticalEventsReportViewModel>().InstancePerLifetimeScope();

            builder.RegisterType<TraceStateReportProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ModelLoader>().InstancePerLifetimeScope();
            builder.RegisterType<ModelFromFileExporter>().InstancePerLifetimeScope();

        }
    }
}