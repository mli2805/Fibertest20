using AutoMapper;

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

            CreateMap<EquipmentIntoNodeAdded, Equipment>();
            CreateMap<EquipmentAtGpsLocationAdded, Equipment>();
            CreateMap<EquipmentUpdated, Equipment>();

            CreateMap<RtuAtGpsLocationAdded, Node>();
            CreateMap<RtuAtGpsLocationAdded, Rtu>();

            CreateMap<OtauAttached, Otau>();

            CreateMap<TraceAdded, Trace>();
            CreateMap<TraceUpdated, Trace>();
        }
    }
}