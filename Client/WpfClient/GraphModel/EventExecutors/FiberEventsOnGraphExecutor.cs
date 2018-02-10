using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class FiberEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;

        public FiberEventsOnGraphExecutor(GraphReadModel model)
        {
            _model = model;
        }

        public void AddFiber(FiberAdded evnt)
        {
            _model.Fibers.Add(new FiberVm()
            {
                Id = evnt.Id,
                Node1 = _model.Nodes.First(m => m.Id == evnt.Node1),
                Node2 = _model.Nodes.First(m => m.Id == evnt.Node2),
            });
        }

        public void RemoveFiber(FiberRemoved evnt)
        {
            _model.Fibers.Remove(_model.Fibers.First(f => f.Id == evnt.Id));
        }
    }
}