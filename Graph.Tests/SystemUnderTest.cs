using System;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using PrivateReflectionUsingDynamic;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        private int _currentEventNumber;
        public int CurrentEventNumber => _currentEventNumber + Aggregate.Events.Count;

        public Guid AddNode()
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddNode
            {
                Id = newGuid
            };

            Aggregate.When(cmd);

            MakeReadModelApplyEventsGeneratedByAggregate();

            return newGuid;
        }

        public void AddRtuAtGpsLocation()
        {
            var cmd = new AddRtuAtGpsLocation();
            Aggregate.When(cmd);
            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        public void AddFiber(Guid left, Guid right)
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddFiber()
            {
                Id = newGuid,
                Node1 = left,
                Node2 = right
            };

            if (Aggregate.When(cmd) != null)
                return;

            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        public void AddFiberWithNodes(Guid left, Guid right, int intermediateNodeCount, EquipmentType equipmentType)
        {
            var cmd = new AddFiberWithNodes()
            {
                Id = Guid.NewGuid(),
                Node1 = left,
                Node2 = right,
                IntermediateNodesCount = intermediateNodeCount,
                EquipmentInIntermediateNodesType = equipmentType,
            };

            if (Aggregate.When(cmd) != null)
                return;

            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        public void MoveNode(Guid id)
        {
            var cmd = new MoveNode() {Id = id};
            Aggregate.When(cmd);
            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        public void AddEquipment()
        {
            var cmd = new AddEquipment();
            Aggregate.When(cmd);
            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        public void UpdateNode(Guid nodeId, string title)
        {
            var cmd = new UpdateNode()
            {
                Id = nodeId,
                Title = title,
            };

            Aggregate.When(cmd);

            MakeReadModelApplyEventsGeneratedByAggregate();
        }

        private void MakeReadModelApplyEventsGeneratedByAggregate()
        {
            foreach (var e in Aggregate.Events)
                ReadModel.AsDynamic().Apply(e);
            _currentEventNumber += Aggregate.Events.Count;
            Aggregate.Events.Clear();
        }
    }
}