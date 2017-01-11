using AutoMapper;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Iit.Fibertest.Graph
{
    public class MappingDomainModelToViewModel : Profile
    {
        public MappingDomainModelToViewModel()
        {
            CreateMap<Equipment, AddEquipmentViewModel>();
        }
    }
}