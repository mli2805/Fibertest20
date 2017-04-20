using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MappingDomainModelToViewModel : Profile
    {
        public MappingDomainModelToViewModel()
        {
            CreateMap<Equipment, EquipmentInfoViewModel>();
        }
    }
}