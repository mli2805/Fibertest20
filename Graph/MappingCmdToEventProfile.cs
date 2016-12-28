using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class MappingCmdToEventProfile : Profile
    {
        public MappingCmdToEventProfile()
        {
            CreateMap<AddNode, NodeAdded>();
            CreateMap<UpdateNode, NodeUpdated>();

            CreateMap<AddEquipment, EquipmentAdded>();

            CreateMap<AddFiber, FiberAdded>();

            CreateMap<AddRtuAtGpsLocation, RtuAddedAtGpsLocation>();

        }
    }
}