using AutoMapper;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class MappingCommandToVm : Profile
    {
        public MappingCommandToVm()
        {
            CreateMap<AddEquipmentIntoNode, EquipmentVm>();
            CreateMap<UpdateEquipment, EquipmentVm>();
            CreateMap<AddTrace, TraceVm>();
        }
    }
}