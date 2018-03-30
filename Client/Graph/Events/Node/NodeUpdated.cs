using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NodeUpdated
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }

    }
}