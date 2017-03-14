using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class MappingDomainModelToViewModel : Profile
    {
        public MappingDomainModelToViewModel()
        {
            CreateMap<Equipment, EquipmentInfoViewModel>();
        }
    }
}