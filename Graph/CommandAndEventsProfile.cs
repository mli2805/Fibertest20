using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class CommandAndEventsProfile : Profile
    {
        public CommandAndEventsProfile()
        {
            CreateMap<AddFiber, FiberAdded>();
            CreateMap<UpdateNode, NodeUpdated>();
        }
    }

    public class EventAndReadModelProfile : Profile
    {
        public EventAndReadModelProfile()
        {
            CreateMap<NodeAdded, Node>();
            CreateMap<NodeUpdated, Node>();
        }
    }
}