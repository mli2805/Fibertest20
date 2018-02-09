using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class MappingCmdToEventProfile : Profile
    {
        public MappingCmdToEventProfile()
        {
            CreateMap<AddNodeIntoFiber, NodeIntoFiberAdded>();
            CreateMap<UpdateNode, NodeUpdated>();
            CreateMap<MoveNode, NodeMoved>();
            CreateMap<RemoveNode, NodeRemoved>();

            CreateMap<AddEquipmentIntoNode, EquipmentIntoNodeAdded>();
            CreateMap<AddEquipmentAtGpsLocation, EquipmentAtGpsLocationAdded>();
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
        }
    }
}