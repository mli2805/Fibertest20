using AutoMapper;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Iit.Fibertest.WpfClient
{
    public class MappingDomainEntityToViewModel : Profile
    {
        public MappingDomainEntityToViewModel()
        {
            CreateMap<Equipment, EquipmentViewModel>();
        }
    }
}