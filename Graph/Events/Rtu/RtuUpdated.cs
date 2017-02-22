using System;

namespace Iit.Fibertest.Graph.Events
{
    [Serializable]
    public class RtuUpdated
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
