using AutoMapper;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Iit.Fibertest.WpfClient
{
    public class MappingDomainModelToViewModel : Profile
    {
        public MappingDomainModelToViewModel()
        {
            CreateMap<Equipment, EquipmentViewModel>();
            CreateMap<EquipmentViewModel, AddEquipment>();
            CreateMap<EquipmentViewModel, UpdateEquipment>();
        }
    }
}