using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Iit.Fibertest.WpfClient
{
    public class MappingViewModeltoDomainEntity : Profile
    {
        public MappingViewModeltoDomainEntity()
        {
            CreateMap<EquipmentViewModel, AddEquipment>();
            CreateMap<EquipmentViewModel, UpdateEquipment>();
        }
    }
}