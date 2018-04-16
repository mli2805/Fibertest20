using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class CurrentZoneRenderer
    {
        private readonly Model _model;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly RenderingResult _renderingResult = new RenderingResult();

        public CurrentZoneRenderer(Model model, IMyLog logFile, CurrentUser currentUser)
        {
            _model = model;
            _logFile = logFile;
            _currentUser = currentUser;
        }

        public RenderingResult Do()
        {
            foreach (var rtu in _model.Rtus.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
                RenderRtus(rtu);

            foreach (var trace in _model.Traces.Where(t => t.ZoneIds.Contains(_currentUser.ZoneId)))
            {
                RenderNodesOfTrace(trace);
                RenderAccidentNodesOfTrace(trace);
                RenderFibersOfTrace(trace);
            }

            _logFile.AppendLine($@"{_renderingResult.NodeVms.Count} nodes ready");
            _logFile.AppendLine($@"{_renderingResult.FiberVms.Count} fibers ready");
            return _renderingResult;
        }

        private void RenderRtus(Rtu rtu)
        {
            var node = _model.Nodes.First(n => n.NodeId == rtu.NodeId);
            _renderingResult.NodeVms.Add(ElementRenderer.Map(node));
        }

        private void RenderFibersOfTrace(Trace trace)
        {
            var fibers = _model.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = _renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId);
                if (fiberVm == null)
                {
                    fiberVm = ElementRenderer.Map(fiber, _renderingResult.NodeVms);
                    _renderingResult.FiberVms.Add(fiberVm);
                }

                if (!fiberVm.States.ContainsKey(trace.TraceId)) // trace could contains loop (pass the same fiber twice or more)
                    fiberVm.States.Add(trace.TraceId, trace.State);
                if (fiber.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId) &&
                    !fiberVm.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                    fiberVm.TracesWithExceededLossCoeff.Add(trace.TraceId, fiber.TracesWithExceededLossCoeff[trace.TraceId]);
            }
        }

        private void RenderAccidentNodesOfTrace(Trace trace)
        {
            foreach (var node in _model.Nodes.Where(n =>
                n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace && n.AccidentOnTraceId == trace.TraceId))
                _renderingResult.NodeVms.Add(ElementRenderer.Map(node));
        }

        private void RenderNodesOfTrace(Trace trace)
        {
            foreach (var nodeId in trace.NodeIds)
            {
                if (_renderingResult.NodeVms.Any(n => n.Id == nodeId)) continue;
                var node = _model.Nodes.First(n => n.NodeId == nodeId);
                _renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
        }

    }
}