using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly ReadModel _readModel;
        private readonly NodeEventsOnGraphExecutor _nodeEventsOnGraphExecutor;
        private readonly TraceEventsOnGraphExecutor _traceEventsOnGraphExecutor;

        public RtuEventsOnGraphExecutor(GraphReadModel model, ReadModel readModel,
            NodeEventsOnGraphExecutor nodeEventsOnGraphExecutor, TraceEventsOnGraphExecutor traceEventsOnGraphExecutor)
        {
            _model = model;
            _readModel = readModel;
            _nodeEventsOnGraphExecutor = nodeEventsOnGraphExecutor;
            _traceEventsOnGraphExecutor = traceEventsOnGraphExecutor;
        }

        public void AddRtuAtGpsLocation(RtuAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
                Title = evnt.Title,
            };
            _model.Data.Nodes.Add(nodeVm);
        }

        public void UpdateRtu(RtuUpdated evnt)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == evnt.RtuId);
            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == rtu.NodeId);
            if (nodeVm == null) return;
            nodeVm.Title = evnt.Title;
        }

        public void RemoveRtu(RtuRemoved evnt)
        {
            foreach (var t in _readModel.Traces.Where(t => t.RtuId == evnt.RtuId).ToList())
                _traceEventsOnGraphExecutor.CleanTrace(new TraceCleaned() { Id = t.Id });

            var rtu = _readModel.Rtus.First(r => r.Id == evnt.RtuId);
            _nodeEventsOnGraphExecutor.RemoveNodeWithAllHis(rtu.NodeId);
        }
    }
}