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
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly OneTraceRenderer _oneTraceRenderer;
        private RenderingResult _renderingResult;

        public CurrentZoneRenderer(Model model, IMyLog logFile, CurrentUser currentUser,
            CurrentlyHiddenRtu currentlyHiddenRtu, OneTraceRenderer oneTraceRenderer)
        {
            _model = model;
            _logFile = logFile;
            _currentUser = currentUser;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _oneTraceRenderer = oneTraceRenderer;
        }

        public RenderingResult GetRendering()
        {
            _renderingResult = new RenderingResult();
            if (_currentUser.Role <= Role.Root)
                RenderAllMinusHiddenTraces();
            else
                RenderVisibleRtusAndTraces();

            _logFile.AppendLine($@"{_renderingResult.NodeVms.Count} nodes ready");
            _logFile.AppendLine($@"{_renderingResult.FiberVms.Count} fibers ready");
            _logFile.AppendLine(@"Rendering finished");

            return _renderingResult;
        }

        private void RenderVisibleRtusAndTraces()
        {
            foreach (var rtu in _model.Rtus.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
                RenderRtus(rtu);

            foreach (var trace in _model.Traces.Where(t => t.ZoneIds.Contains(_currentUser.ZoneId) && !_currentlyHiddenRtu.Collection.Contains(t.RtuId)))
            {
                _oneTraceRenderer.GetRendering(trace, _renderingResult);
            }
        }

        private void RenderAllMinusHiddenTraces()
        {
            RenderAll();

            MinusHiddenTraces();
        }

        private void MinusHiddenTraces()
        {
            foreach (var trace in _model.Traces.Where(t => _currentlyHiddenRtu.Collection.Contains(t.RtuId)))
            {
                var fibers = _model.GetTraceFibers(trace).ToList();
                foreach (var fiber in fibers)
                {
                    var fiberVm = _renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId);
                    if (fiberVm == null) continue;
                    if (fiberVm.States.ContainsKey(trace.TraceId))
                        fiberVm.States.Remove(trace.TraceId);
                    if (fiberVm.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                        fiberVm.TracesWithExceededLossCoeff.Remove(trace.TraceId);

                    if (fiberVm.States.Count == 0)
                        _renderingResult.FiberVms.Remove(fiberVm);
                }

                foreach (var nodeId in trace.NodeIds)
                {
                    var nodeVm = _renderingResult.NodeVms.FirstOrDefault(n => n.Id == nodeId);
                    if (nodeVm == null) continue;
                    if (nodeVm.Type == EquipmentType.Rtu) continue;
                    if (_renderingResult.FiberVms.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId)) continue;
                    _renderingResult.NodeVms.Remove(nodeVm);
                }

                foreach (var nodeVm in _renderingResult.NodeVms.Where(n => n.AccidentOnTraceVmId == trace.TraceId).ToList())
                    _renderingResult.NodeVms.Remove(nodeVm);
            }
        }

        private void RenderAll()
        {
            foreach (var node in _model.Nodes)
                _renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            foreach (var fiber in _model.Fibers)
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, _renderingResult.NodeVms);
                if (fiberVm != null)
                    _renderingResult.FiberVms.Add(fiberVm);
            }
        }

        private void RenderRtus(Rtu rtu)
        {
            var node = _model.Nodes.First(n => n.NodeId == rtu.NodeId);
            _renderingResult.NodeVms.Add(ElementRenderer.Map(node));
        }


    }
}