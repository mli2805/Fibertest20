using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RootRenderAndApply
    {
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;

        public RootRenderAndApply(Model readModel, GraphReadModel graphReadModel)
        {
            _readModel = readModel;
            _graphReadModel = graphReadModel;
        }

        public void ShowAll()
        {
            foreach (var node in _readModel.Nodes)
                _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers)
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _graphReadModel.Data.Nodes);
                if (fiberVm != null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
            }
        }

        public void HideAll()
        {
            foreach (var node in _readModel.Nodes)
                if (_readModel.Rtus.Any(r => r.NodeId == node.NodeId) ||
                    !_readModel.Traces.Any(t => t.NodeIds.Contains(node.NodeId)))
                    _graphReadModel.Data.Nodes.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers.Where(f => f.States.Count == 0))
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _graphReadModel.Data.Nodes);
                if (fiberVm != null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
            }
        }

    }
}