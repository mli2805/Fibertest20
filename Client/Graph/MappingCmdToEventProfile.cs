using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class MappingCmdToEventProfile : Profile
    {
        public MappingCmdToEventProfile()
        {
            CreateMap<AddNodeIntoFiber, NodeIntoFiberAdded>();
            CreateMap<UpdateNode, NodeUpdated>();
            CreateMap<UpdateAndMoveNode, NodeUpdatedAndMoved>();
            CreateMap<MoveNode, NodeMoved>();
            CreateMap<RemoveNode, NodeRemoved>();

            CreateMap<AddEquipmentIntoNode, EquipmentIntoNodeAdded>();
            CreateMap<AddEquipmentAtGpsLocation, EquipmentAtGpsLocationAdded>();
            CreateMap<AddEquipmentAtGpsLocationWithNodeTitle, EquipmentAtGpsLocationWithNodeTitleAdded>();
            CreateMap<UpdateEquipment, EquipmentUpdated>();
            CreateMap<RemoveEquipment, EquipmentRemoved>();

            CreateMap<AddFiber, FiberAdded>();
            CreateMap<UpdateFiber, FiberUpdated>();
            CreateMap<RemoveFiber, FiberRemoved>();

            CreateMap<AddRtuAtGpsLocation, RtuAtGpsLocationAdded>();
            CreateMap<UpdateRtu, RtuUpdated>();
            CreateMap<RemoveRtu, RtuRemoved>();
            CreateMap<AttachOtau, OtauAttached>();
            CreateMap<DetachOtau, OtauDetached>();

            CreateMap<InitializeRtu, RtuInitialized>();
            CreateMap<ChangeMonitoringSettings, MonitoringSettingsChanged>();
            CreateMap<AssignBaseRef, BaseRefAssigned>();
            CreateMap<StartMonitoring, MonitoringStarted>();
            CreateMap<StopMonitoring, MonitoringStopped>();

            CreateMap<AttachTrace, TraceAttached>();
            CreateMap<DetachTrace, TraceDetached>();

            CreateMap<AddTrace, TraceAdded>();
            CreateMap<UpdateTrace, TraceUpdated>();
            CreateMap<CleanTrace, TraceCleaned>();
            CreateMap<RemoveTrace, TraceRemoved>();
            CreateMap<AssignBaseRef, BaseRefAssigned>();

            CreateMap<AddZone, ZoneAdded>();
            CreateMap<UpdateZone, ZoneUpdated>();
            CreateMap<RemoveZone, ZoneRemoved>();

            CreateMap<AddUser, UserAdded>();
            CreateMap<UpdateUser, UserUpdated>();
            CreateMap<RemoveUser, UserRemoved>();
            CreateMap<SaveUsersHiddentRtus, UsersHiddenRtusSaved>();

            CreateMap<AddMeasurement, MeasurementAdded>();
            CreateMap<UpdateMeasurement, MeasurementUpdated>();
            CreateMap<AddNetworkEvent, NetworkEventAdded>();
            CreateMap<AddBopNetworkEvent, BopNetworkEventAdded>();
            CreateMap<ChangeResponsibilities, ResponsibilitiesChanged>();
        }
    }
}