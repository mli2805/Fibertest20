using AutoMapper;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class MappingVmToViewModel : Profile
    {
        public MappingVmToViewModel()
        {
            CreateMap<EquipmentVm, EquipmentUpdateViewModel>();
        }
    }
}