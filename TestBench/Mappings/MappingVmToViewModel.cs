using AutoMapper;

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