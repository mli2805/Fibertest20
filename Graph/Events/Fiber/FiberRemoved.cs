using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class FiberRemoved
    {
        public Guid Id { get; set; }
    }
}
