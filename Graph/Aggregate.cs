using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {
        public List<object> Events { get; } = new List<object>();
        public readonly HashSet<NodePairKey> FibersByNodePairs = new HashSet<NodePairKey>();
        public readonly HashSet<string> NodeTitles = new HashSet<string>();
        public void When(AddNode cmd)
        {
            Events.Add(new NodeAdded
            {
                Id = cmd.Id,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
            });
        }

        public void When(AddFiber cmd)
        {
            if (!FibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                throw new Exception("already exists");

            Events.Add(new FiberAdded
            {
                Id = cmd.Id,
                Node1 = cmd.Node1,
                Node2 = cmd.Node2,
            });
        }

        public void When(RemoveNode cmd)
        {
            Events.Add(new NodeRemoved
            {
                Id = cmd.Id,
            });
        }
        public void When(UpdateNode cmd)
        {
            if (!NodeTitles.Add(cmd.Title))
                throw new Exception("node title already exists");
            Events.Add(new NodeUpdated()
            {
                Id = cmd.Id,
                Title = cmd.Title,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
            });
        }
    }

    public struct NodePairKey
    {
        private readonly Guid _a;
        private readonly Guid _b;

        public NodePairKey(Guid a, Guid b)
        {
            if (string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal) < 0)
            {
                _a = b;
                _b = a;
            }
            else
            {
                _a = a;
                _b = b;
            }
        }
    }
}
