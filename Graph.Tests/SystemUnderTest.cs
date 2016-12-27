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

            Apply();

            return newGuid;
        }

        private void Apply()
        {
            foreach (var e in Aggregate.Events)
                ReadModel.AsDynamic().Apply(e);
            _currentEventNumber += Aggregate.Events.Count;
            Aggregate.Events.Clear();
        }

        public void UpdateNode(Guid nodeId, string title)
        {
            var cmd = new UpdateNode()
            {
                Id = nodeId,
                Title = title,
            };

            Aggregate.When(cmd);

            Apply();
        }
    }
}