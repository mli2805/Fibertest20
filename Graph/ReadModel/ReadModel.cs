using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class ReadModel
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<EventAndReadModelProfile>()).CreateMapper();

        public List<Node> Nodes { get; } = new List<Node>();
        public void Apply(NodeAdded e)
        {
            Nodes.Add(_mapper.Map<Node>(e));
        }
        public void Apply(NodeUpdated e)
        {
            _mapper.Map(e, Nodes.Single(n => n.Id == e.Id));
        }
        public void Apply(NodeRemoved e)
        {
            Nodes.Remove(Nodes.Single(n=>n.Id == e.Id));
        }

    }
}