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
            CreateMap<FiberWithNodesAdded, Fiber>();

            CreateMap<EquipmentAdded, Equipment>();

            CreateMap<RtuAddedAtGpsLocation, Rtu>();
        }
    }
}