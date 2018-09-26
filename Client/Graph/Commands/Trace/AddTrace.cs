using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class AddTrace
    {
        public Guid TraceId { get; set; }
        public Guid RtuId { get; set; }
        public string Title { get; set; }
        public List<Guid> NodeIds { get; set; } = new List<Guid>();
        public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
        public List<Guid> FiberIds { get; set; } = new List<Guid>();
        public string Comment { get; set; }
    }
}
