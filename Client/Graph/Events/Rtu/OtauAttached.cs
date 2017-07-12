using System;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class OtauAttached
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }

        public NetAddress NetAddress { get; set; } = new NetAddress();
        public RtuPartState NetAddressState { get; set; }
        public int Serial { get; set; }
        public int PortCount { get; set; }

        public int FirstPortNumber { get; set; }
        public int MasterPort { get; set; }
    }
}
