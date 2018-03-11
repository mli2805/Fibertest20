using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly NodeEventsOnGraphExecutor _nodeEventsOnGraphExecutor;
        private readonly TraceEventsOnGraphExecutor _traceEventsOnGraphExecutor;

        public RtuEventsOnGraphExecutor(GraphReadModel model, 
            NodeEventsOnGraphExecutor nodeEventsOnGraphExecutor, TraceEventsOnGraphExecutor traceEventsOnGraphExecutor)
        {
            _model = model;
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

            var rtuVm = new RtuVm() { Id = evnt.Id, Node = nodeVm, Title = evnt.Title, Comment = evnt.Comment, };
            _model.Data.Rtus.Add(rtuVm);
        }

        public void UpdateRtu(RtuUpdated evnt)
        {
            var rtu = _model.Data.Rtus.FirstOrDefault(r => r.Id == evnt.Id);
            if (rtu == null)
                return;
            rtu.Title = evnt.Title;
            rtu.Node.Title = evnt.Title;
        }

        public void RemoveRtu(RtuRemoved evnt)
        {
            var rtuVm = _model.Data.Rtus.FirstOrDefault(r => r.Id == evnt.Id);
            if (rtuVm == null) return;
            Guid nodeId = rtuVm.Node.Id;
            foreach (var t in _model.Data.Traces.Where(t => t.RtuId == rtuVm.Id).ToList())
            {
                _traceEventsOnGraphExecutor.CleanTrace(new TraceCleaned() { Id = t.Id });
                _model.Data.Traces.Remove(t);
            }
            _model.Data.Rtus.Remove(rtuVm);
            _nodeEventsOnGraphExecutor.RemoveNodeWithAllHis(nodeId);
        }
    }
}