using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Events
{
    public class TraceAdded
    {
        public Guid Id { get; set; }
        public List<Guid> Nodes { get; set; } = new List<Guid>();
        public List<Guid> Equipments { get; set; } = new List<Guid>();
    }
}
