using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Model 
    {
        public License License { get; set; }
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Fiber> Fibers { get; } = new List<Fiber>();
        public List<Equipment> Equipments { get; } = new List<Equipment>();
        public List<Rtu> Rtus { get; } = new List<Rtu>();
        public List<Trace> Traces { get; } = new List<Trace>();
        public List<Otau> Otaus { get; } = new List<Otau>();
        public List<User> Users { get; } = new List<User>();
        public List<Zone> Zones { get; } = new List<Zone>();
        public List<Measurement> Measurements { get; } = new List<Measurement>();
        public List<NetworkEvent> NetworkEvents { get; } = new List<NetworkEvent>();
        public List<BopNetworkEvent> BopNetworkEvents { get; } = new List<BopNetworkEvent>();
        public List<BaseRef> BaseRefs { get; } = new List<BaseRef>();
    }
}