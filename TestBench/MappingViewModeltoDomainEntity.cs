using AutoMapper;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
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