using System;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public sealed class MapViewModel
    {
        private readonly Aggregate _aggregate;

        public MapViewModel(Aggregate aggregate)
        {
            _aggregate = aggregate;
        }

        public Guid AddFiber(Guid left, Guid right)
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddFiber()
            {
                Id = newGuid,
                Node1 = left,
                Node2 = right
            };

            if (_aggregate.When(cmd) != null)
                return Guid.Empty;

            return newGuid;
        }
    }
}