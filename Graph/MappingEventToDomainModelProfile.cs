using AutoMapper;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class MappingEventToDomainModelProfile : Profile
    {
        public MappingEventToDomainModelProfile()
        {
            CreateMap<NodeAdded, Node>();
            CreateMap<NodeIntoFiberAdded, Node>();
            CreateMap<NodeUpdated, Node>();
            CreateMap<NodeMoved, Node>();

            CreateMap<FiberAdded, Fiber>();
            CreateMap<FiberWithNodesAdded, Fiber>();
            CreateMap<FiberRemoved, Fiber>();

            CreateMap<EquipmentAdded, Equipment>();
            CreateMap<EquipmentAtGpsLocationAdded, Equipment>();

            CreateMap<RtuAtGpsLocationAdded, Rtu>();

            CreateMap<TraceAdded, Trace>();
        }
    }
}