using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class MappingDomainEntityToViewModel : Profile
    {
        public MappingDomainEntityToViewModel()
        {
            CreateMap<Equipment, EquipmentUpdateViewModel>();
        }
    }
}