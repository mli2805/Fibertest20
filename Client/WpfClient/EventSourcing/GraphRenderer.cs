using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GraphRenderer
    {
        private readonly ReadModel _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentUser _currentUser;

        private readonly List<NodeVm> _nodesForRendering = new List<NodeVm>();
        private readonly List<FiberVm> _fibersForRendering = new List<FiberVm>();

        public GraphRenderer(ReadModel readModel, GraphReadModel graphReadModel, CurrentUser currentUser)
        {
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _currentUser = currentUser;
        }

        public void Do()
        {
            if (_readModel.Zones.First(z=>z.ZoneId == _currentUser.ZoneId).IsDefaultZone)
                RenderAllGraph();
            else RenderOneZone();
        }

        private void RenderOneZone()
        {
            foreach (var trace in _readModel.Traces.Where(t=>t.ZoneIds.Contains(_currentUser.ZoneId)))
            {
                foreach (var nodeId in trace.Nodes)
                {
                    if (_nodesForRendering.Any(n=>n.Id == nodeId)) continue;
                    var node = _readModel.Nodes.First(n => n.Id == nodeId);
                    _nodesForRendering.Add(Map(node));
                }

                foreach (var node in _readModel.Nodes.Where(n=>n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace && n.AccidentOnTraceId == trace.Id))
                    _nodesForRendering.Add(Map(node));

                var fibers = _readModel.GetTraceFibers(trace);
                foreach (var fiber in fibers)
                {
                    var fiberVm =_fibersForRendering.FirstOrDefault(f=>f.Id == fiber.Id);
                    if (fiberVm == null)
                        fiberVm = Map(fiber);
                    fiberVm.States.Add(trace.Id, trace.State);
                    if (fiber.TracesWithExceededLossCoeff.Contains(trace.Id))
                        fiberVm.TracesWithExceededLossCoeff.Add(trace.Id);
                    _fibersForRendering.Add(fiberVm);
                }
            }

            Transfer();
        }

        private void RenderAllGraph()
        {
            foreach (var node in _readModel.Nodes)
                _nodesForRendering.Add(Map(node));

            foreach (var fiber in _readModel.Fibers)
                _fibersForRendering.Add(MapWithAllTraceStates(fiber));

            Transfer();
        }

        private void Transfer()
        {
            foreach (var nodeVm in _nodesForRendering)
                _graphReadModel.Data.Nodes.Add(nodeVm);
            foreach (var fiberVm in _fibersForRendering)
                _graphReadModel.Data.Fibers.Add(fiberVm);
        }


        private NodeVm Map(Node node)
        {
            return new NodeVm()
            {
                Id = node.Id,
                Title = node.Title,
                Position = node.Position,
                Type = node.TypeOfLastAddedEquipment,
            };
        }
        private FiberVm Map(Fiber fiber)
        {
            return new FiberVm()
            {
                Id = fiber.Id,
                Node1 = _nodesForRendering.First(n => n.Id == fiber.Node1),
                Node2 = _nodesForRendering.First(n => n.Id == fiber.Node2),
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