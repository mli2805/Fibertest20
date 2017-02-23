using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class MappingCommandToVm : Profile
    {
        // все должно уехать в MappingEventToVm
        public MappingCommandToVm()
        {
            CreateMap<AddEquipmentIntoNode, EquipmentVm>();
            CreateMap<UpdateEquipment, EquipmentVm>();
        }
    }

    public class MappingEventToVm : Profile
    {
        public MappingEventToVm()
        {
            CreateMap<TraceAdded, TraceVm>();
        }
    }
}