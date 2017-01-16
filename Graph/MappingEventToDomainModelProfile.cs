using AutoMapper;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class MappingEventToDomainModelProfile : Profile
    {
        public MappingEventToDomainModelProfile()
        {
            CreateMap<NodeAdded, Node>();
            CreateMap<NodeUpdated, Node>();
            CreateMap<NodeMoved, Node>();

            CreateMap<FiberAdded, Fiber>();
            CreateMap<FiberUpdated, Fiber>();
            CreateMap<FiberRemoved, Fiber>();

            CreateMap<EquipmentAdded, Equipment>();
            CreateMap<EquipmentAtGpsLocationAdded, Equipment>();
            CreateMap<EquipmentUpdated, Equipment>();

            CreateMap<RtuAtGpsLocationAdded, Node>();
            CreateMap<RtuAtGpsLocationAdded, Rtu>();

            CreateMap<TraceAdded, Trace>();
            CreateMap<BaseRefAssigned, BaseRef>();
        }
    }
}