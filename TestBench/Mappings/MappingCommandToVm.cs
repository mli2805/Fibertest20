using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class MappingCommandToVm : Profile
    {
        // ��� ������ ������ � MappingEventToVm
        public MappingCommandToVm()
        {
            CreateMap<AddEquipmentIntoNode, EquipmentVm>();
        }
    }

    public class MappingEventToVm : Profile
    {
        public MappingEventToVm()
        {
            CreateMap<TraceAdded, TraceVm>();
            CreateMap<EquipmentUpdated, EquipmentVm>();
        }
    }
}