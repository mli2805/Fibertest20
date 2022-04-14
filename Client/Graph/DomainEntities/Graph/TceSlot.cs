using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TceSlot
    {
        public int Position { get; set; }
        public int GponInterfaceCount { get; set; }
        public bool IsPresent => GponInterfaceCount > 0;
    }
}