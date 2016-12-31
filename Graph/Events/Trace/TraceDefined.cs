using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Events
{
    public class TraceDefined
    {
        public Guid Id { get; set; }
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Equipment> Equipments { get; } = new List<Equipment>();
    }
}
