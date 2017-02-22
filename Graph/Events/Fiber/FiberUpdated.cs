using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class FiberUpdated
    {
        public Guid Id { get; set; }
        public double UserInputedLength { get; set; }
    }
}
