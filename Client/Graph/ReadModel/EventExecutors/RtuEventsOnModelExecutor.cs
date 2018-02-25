using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class RtuEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;

        public RtuEventsOnModelExecutor(ReadModel model)
        {
            _model = model;
        }
        public void AddRtuAtGpsLocation(RtuAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude };
            _model.Nodes.Add(node);
            Rtu rtu = _mapper.Map<Rtu>(e);
            _model.Rtus.Add(rtu);

        }

        public void UpdateRtu(RtuUpdated e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
                return;
            rtu.Title = e.Title;
            rtu.Comment = e.Comment;
        }

        public void RemoveRtu(RtuRemoved e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
                return;
            var nodeId = rtu.NodeId;
            _model.Traces.RemoveAll(t => t.RtuId == rtu.Id);
            _model.Rtus.Remove(rtu);
            _model.RemoveNodeWithAllHis(nodeId);
        }

        public void AttachOtau(OtauAttached e)
        {
            Otau otau = _mapper.Map<Otau>(e);
            _model.Otaus.Add(otau);
        }

        public void DetachOtau(OtauDetached e)
        {
            var otau =  _model.Otaus.FirstOrDefault(o => o.Id == e.Id);
            if (otau == null)
                return;
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
                return;

            rtu.FullPortCount -= otau.PortCount;
            _model.Otaus.Remove(otau);
        }
    }
}