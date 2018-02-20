﻿using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class MappingEventToDomainModelProfile : Profile
    {
        public MappingEventToDomainModelProfile()
        {
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
            CreateMap<RtuInitialized, Rtu>();

            CreateMap<OtauAttached, Otau>();

            CreateMap<TraceAdded, Trace>();
            CreateMap<TraceUpdated, Trace>();

            CreateMap<ZoneAdded, Zone>();
            CreateMap<ZoneUpdated, Zone>();

            CreateMap<UserAdded, User>();
            CreateMap<UserUpdated, User>();
        }

        
    }

}