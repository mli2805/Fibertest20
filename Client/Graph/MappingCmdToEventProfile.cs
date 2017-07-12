using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class MappingCmdToEventProfile : Profile
    {
        public MappingCmdToEventProfile()
        {
            CreateMap<AddNode, NodeAdded>();
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
            CreateMap<InitializeRtu, RtuInitialized>();
            CreateMap<UpdateRtu, RtuUpdated>();
            CreateMap<RemoveRtu, RtuRemoved>();
            CreateMap<AttachOtau, OtauAttached>();
            CreateMap<DetachOtau, OtauDetached>();

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