﻿using System;
using System.Collections.Generic;
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
        private RenderingResult _renderingResult;
        private List<Guid> _hiddenRtus;

        public CurrentZoneRenderer(Model model, IMyLog logFile, CurrentUser currentUser)
        {
            _model = model;
            _logFile = logFile;
            _currentUser = currentUser;
        }

        public RenderingResult GetRendering()
        {
            _renderingResult = new RenderingResult();
            if  (_currentUser.Role <= Role.Root)
                RenderAllMinusHiddenTraces();
            else
                RenderVisibleRtusAndTraces();

            _logFile.AppendLine($@"{_renderingResult.NodeVms.Count} nodes ready");
            _logFile.AppendLine($@"{_renderingResult.FiberVms.Count} fibers ready");

            return _renderingResult;
        }

        private void RenderVisibleRtusAndTraces()
        {
            _hiddenRtus = _model.Users.First(u => u.UserId == _currentUser.UserId).HiddenRtus;

            foreach (var rtu in _model.Rtus.Where(r => r.ZoneIds.Contains(_currentUser.ZoneId)))
                RenderRtus(rtu);

            foreach (var trace in _model.Traces.Where(t => t.ZoneIds.Contains(_currentUser.ZoneId) && !_hiddenRtus.Contains(t.RtuId)))
            {
                RenderNodesOfTrace(trace);
                RenderAccidentNodesOfTrace(trace);
                RenderFibersOfTrace(trace);
            }
        }

        private void RenderAllMinusHiddenTraces()
        {
            RenderAll();

            MinusHiddenTraces();
        }

        private void MinusHiddenTraces()
        {
            _hiddenRtus = _model.Users.First(u => u.UserId == _currentUser.UserId).HiddenRtus;
            foreach (var trace in _model.Traces.Where(t => _hiddenRtus.Contains(t.RtuId)))
            {
                var fibers = _model.GetTraceFibers(trace).ToList();
                foreach (var fiber in fibers)
                {
                    var fiberVm = _renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId);
                    if (fiberVm == null) continue;
                    if (fiberVm.States.Count > 1)
                    {
                        fiberVm.States.Remove(trace.TraceId);
                        if (fiberVm.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                            fiberVm.TracesWithExceededLossCoeff.Remove(trace.TraceId);
                    }
                    else _renderingResult.FiberVms.Remove(fiberVm);
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

        private void RenderFibersOfTrace(Trace trace)
        {
            var fibers = _model.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = _renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId); // prevent repeating fibers if trace has loop
                if (fiberVm == null)
                {
                    fiberVm = ElementRenderer.Map(fiber, _renderingResult.NodeVms);
                    if (fiberVm == null) continue; // something goes wrong, nodeVms not found to define fiberVm

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
                var node = _model.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node != null)
                    _renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
        }

    }
}