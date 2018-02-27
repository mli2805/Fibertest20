 using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MappingEventToVm : Profile
    {
        public MappingEventToVm()
        {
            CreateMap<TraceAdded, TraceVm>();
            CreateMap<EquipmentIntoNodeAdded, EquipmentVm>();
            CreateMap<EquipmentUpdated, EquipmentVm>();
        }
    }
}