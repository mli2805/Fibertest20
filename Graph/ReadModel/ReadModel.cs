using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class ReadModel
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public List<Node> Nodes { get; } = new List<Node>();
        public void Apply(NodeAdded e)
        {
            Node node = _mapper.Map<Node>(e);
            Nodes.Add(node);
        }

        public void Apply(NodeUpdated source)
        {
            Node destination = Nodes.Single(n => n.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(NodeRemoved e)
        {
            Node node = Nodes.Single(n=>n.Id == e.Id);
            Nodes.Remove(node);
        }
    }
}