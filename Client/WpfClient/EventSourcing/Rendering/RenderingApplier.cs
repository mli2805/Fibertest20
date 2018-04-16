using System.Linq;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class RenderingApplier
    {
        private readonly IMyLog _logFile;
        private readonly GraphReadModel _graphReadModel;

        public RenderingApplier(IMyLog logFile, GraphReadModel graphReadModel)
        {
            _logFile = logFile;
            _graphReadModel = graphReadModel;
        }

        public void ToEmptyGraph(RenderingResult renderingResult)
        {
            _logFile.AppendLine($@"{renderingResult.NodeVms.Count} nodes ready");
            foreach (var nodeVm in renderingResult.NodeVms)
                _graphReadModel.Data.Nodes.Add(nodeVm);

            _logFile.AppendLine($@"{renderingResult.FiberVms.Count} fibers ready");
            foreach (var fiberVm in renderingResult.FiberVms)
                _graphReadModel.Data.Fibers.Add(fiberVm);
        }

        public void ToExistingGraph(RenderingResult renderingResult)
        {
            // remove nodes for deleted traces
            foreach (var nodeVm in _graphReadModel.Data.Nodes.ToList())
            {
                if (renderingResult.NodeVms.All(n => n.Id != nodeVm.Id))
                    _graphReadModel.Data.Nodes.Remove(nodeVm);
            }

            // add nodes for added traces
            foreach (var nodeVm in renderingResult.NodeVms)
            {
                if (_graphReadModel.Data.Nodes.All(n => n.Id != nodeVm.Id))
                    _graphReadModel.Data.Nodes.Add(nodeVm);
            }

            // remove fibers for deleted traces
            foreach (var fiberVm in _graphReadModel.Data.Fibers.ToList())
            {
                if (renderingResult.FiberVms.All(f => f.Id != fiberVm.Id))
                    _graphReadModel.Data.Fibers.Remove(fiberVm);
            }

            // add fibers for added traces
            foreach (var fiberVm in renderingResult.FiberVms)
            {
                var oldFiberVm = _graphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiberVm.Id);
                if (oldFiberVm == null)
                    _graphReadModel.Data.Fibers.Add(fiberVm);
                else
                {
                    // though fiber exists already it's characteristics could be changed
                    if (oldFiberVm.TracesWithExceededLossCoeff.Any() ^ fiberVm.TracesWithExceededLossCoeff.Any()
                        || oldFiberVm.State != fiberVm.State)
                    {
                        _graphReadModel.Data.Fibers.Remove(oldFiberVm);
                        _graphReadModel.Data.Fibers.Add(fiberVm);
                    }
                    else
                    {
                        oldFiberVm.States = fiberVm.States;
                        oldFiberVm.TracesWithExceededLossCoeff = fiberVm.TracesWithExceededLossCoeff;
                    }
                }
            }
        }

    }
}