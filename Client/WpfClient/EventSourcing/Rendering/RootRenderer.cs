using System;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RootRenderer
    {
        private readonly Model _readModel;

        public RootRenderer(Model readModel)
        {
            _readModel = readModel;
        }

        public RenderingResult ShowAll()
        {
            var renderingResult = new RenderingResult();

            foreach (var node in _readModel.Nodes)
                renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers)
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, renderingResult.NodeVms);
                if (fiberVm != null)
                    renderingResult.FiberVms.Add(fiberVm);
            }

            return renderingResult;
        }

        public RenderingResult ShowOnlyRtusAndNotInTraces() // HideAll()
        {
            var renderingResult = new RenderingResult();

            foreach (var node in _readModel.Nodes)
                if (_readModel.Rtus.Any(r => r.NodeId == node.NodeId) ||
                    (node.AccidentOnTraceId == Guid.Empty && !_readModel.Traces.Any(t => t.NodeIds.Contains(node.NodeId))))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers.Where(f => f.States.Count == 0))
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, renderingResult.NodeVms);
                if (fiberVm != null)
                    renderingResult.FiberVms.Add(fiberVm);
            }

            return renderingResult;
        }
    }
}