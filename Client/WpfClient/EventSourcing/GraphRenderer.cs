using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GraphRenderer
    {
        private readonly Model _model;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentUser _currentUser;

        private List<NodeVm> _nodesForRendering = new List<NodeVm>();
        private List<FiberVm> _fibersForRendering = new List<FiberVm>();

        public GraphRenderer(Model model, GraphReadModel graphReadModel, CurrentUser currentUser)
        {
            _model = model;
            _graphReadModel = graphReadModel;
            _currentUser = currentUser;
        }

        public void RenderGraphOnApplicationStart()
        {
            if (_model.Zones.First(z => z.ZoneId == _currentUser.ZoneId).IsDefaultZone)
                RenderAllGraph();
            else RenderOneZone();

            Transfer();
        }

        public void ReRenderOneZoneOnResponsibilitiesChanged()
        {
            _nodesForRendering = new List<NodeVm>();
            _fibersForRendering = new List<FiberVm>();

            RenderOneZone();

            ApplyToExistingGraph();
        }


        private void RenderOneZone()
        {
            foreach (var trace in _model.Traces.Where(t => t.ZoneIds.Contains(_currentUser.ZoneId)))
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    if (_nodesForRendering.Any(n => n.Id == nodeId)) continue;
                    var node = _model.Nodes.First(n => n.NodeId == nodeId);
                    _nodesForRendering.Add(Map(node));
                }

                foreach (var node in _model.Nodes.Where(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace && n.AccidentOnTraceId == trace.TraceId))
                    _nodesForRendering.Add(Map(node));

                var fibers = _model.GetTraceFibers(trace);
                foreach (var fiber in fibers)
                {
                    var fiberVm = _fibersForRendering.FirstOrDefault(f => f.Id == fiber.FiberId);
                    if (fiberVm == null)
                        fiberVm = Map(fiber);
                    fiberVm.States.Add(trace.TraceId, trace.State);
                    if (fiber.TracesWithExceededLossCoeff.Contains(trace.TraceId))
                        fiberVm.TracesWithExceededLossCoeff.Add(trace.TraceId);
                    _fibersForRendering.Add(fiberVm);
                }
            }
        }

        private void RenderAllGraph()
        {
            foreach (var node in _model.Nodes)
                _nodesForRendering.Add(Map(node));

            foreach (var fiber in _model.Fibers)
                _fibersForRendering.Add(MapWithAllTraceStates(fiber));
        }

        private void Transfer()
        {
            foreach (var nodeVm in _nodesForRendering)
                _graphReadModel.Data.Nodes.Add(nodeVm);
            foreach (var fiberVm in _fibersForRendering)
                _graphReadModel.Data.Fibers.Add(fiberVm);
        }

        private void ApplyToExistingGraph()
        {
            // remove nodes for deleted traces
            foreach (var nodeVm in _graphReadModel.Data.Nodes.ToList())
            {
                if (_nodesForRendering.All(n => n.Id != nodeVm.Id))
                    _graphReadModel.Data.Nodes.Remove(nodeVm);
            }

            // add nodes for added traces
            foreach (var nodeVm in _nodesForRendering)
            {
                if (_graphReadModel.Data.Nodes.All(n => n.Id != nodeVm.Id))
                    _graphReadModel.Data.Nodes.Add(nodeVm);
            }

            // remove fibers for deleted traces
            foreach (var fiberVm in _graphReadModel.Data.Fibers.ToList())
            {
                if (_fibersForRendering.All(f => f.Id != fiberVm.Id))
                    _graphReadModel.Data.Fibers.Remove(fiberVm);
            }

            // add fibers for added traces
            foreach (var fiberVm in _fibersForRendering)
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

        private NodeVm Map(Node node)
        {
            return new NodeVm()
            {
                Id = node.NodeId,
                Title = node.Title,
                Position = node.Position,
                Type = node.TypeOfLastAddedEquipment,
                AccidentOnTraceVmId = node.AccidentOnTraceId,
                State = node.State,
            };
        }
        private FiberVm Map(Fiber fiber)
        {
            return new FiberVm()
            {
                Id = fiber.FiberId,
                Node1 = _nodesForRendering.First(n => n.Id == fiber.NodeId1),
                Node2 = _nodesForRendering.First(n => n.Id == fiber.NodeId2),
                States = new Dictionary<Guid, FiberState>(),
            };
        }

        private FiberVm MapWithAllTraceStates(Fiber fiber)
        {
            var fiberVm = Map(fiber);
            foreach (var pair in fiber.States)
                fiberVm.States.Add(pair.Key, pair.Value);
            fiber.TracesWithExceededLossCoeff.ForEach(e => fiberVm.TracesWithExceededLossCoeff.Add(e));
            return fiberVm;
        }
    }
}