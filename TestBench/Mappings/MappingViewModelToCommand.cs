using AutoMapper;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class MappingViewModelToCommand : Profile
    {
        public MappingViewModelToCommand()
        {
            CreateMap<EquipmentUpdateViewModel, AddEquipmentIntoNode>();
            CreateMap<EquipmentUpdateViewModel, UpdateEquipment>();
            CreateMap<RtuUpdateViewModel, UpdateRtu>();
        }
    }
}