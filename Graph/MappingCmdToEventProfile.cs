using System.Runtime.InteropServices;
using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

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

            CreateMap<AddEquipment, EquipmentAdded>();
            CreateMap<AddEquipmentAtGpsLocation, EquipmentAtGpsLocationAdded>();

            CreateMap<AddFiber, FiberAdded>();
            CreateMap<AddFiberWithNodes, FiberWithNodesAdded>();
            CreateMap<UpdateFiber, FiberUpdated>();
            CreateMap<RemoveFiber, FiberRemoved>();

            CreateMap<AddRtuAtGpsLocation, RtuAtGpsLocationAdded>();
            CreateMap<AttachTrace, TraceAttached>();
            CreateMap<DetachTrace, TraceDetached>();

            CreateMap<AddTrace, TraceAdded>();
            CreateMap<AssignBaseRef, BaseRefAssigned>();

        }
    }
}