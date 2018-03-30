using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class ReadModel : PropertyChangedBase, IModel
    {
        public IMyLog LogFile { get; }

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


        public int JustForNotification { get; set; }

        public ReadModel(IMyLog logFile)
        {
            LogFile = logFile;
        }

        public bool HasFiberBetween(Guid a, Guid b)
        {
            return Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a);
        }

        public string RemoveNodeWithAllHis(Guid nodeId)
        {
            Fibers.RemoveAll(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
            Equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                Nodes.Remove(node);
                return null;
            }

            var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

    }
}