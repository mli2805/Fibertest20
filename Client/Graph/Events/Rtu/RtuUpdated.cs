using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuUpdated
    {
        public Guid RtuId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
