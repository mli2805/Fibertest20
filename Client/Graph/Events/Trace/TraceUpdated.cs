using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TraceUpdated
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TraceMode Mode { get; set; }
        public string Comment { get; set; }
    }
}