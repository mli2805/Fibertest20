using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class RtuEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;
        private readonly IMyLog _logFile;

        public RtuEventsOnModelExecutor(ReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }
        public string AddRtuAtGpsLocation(RtuAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude), TypeOfLastAddedEquipment = EquipmentType.Rtu };
            _model.Nodes.Add(node);
            Rtu rtu = _mapper.Map<Rtu>(e);
            _model.Rtus.Add(rtu);
            return null;
        }

        public string UpdateRtu(RtuUpdated e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
            {
                var message = $@"RtuUpdated: RTU {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            rtu.Title = e.Title;
            rtu.Comment = e.Comment;
            return null;
        }

        public string RemoveRtu(RtuRemoved e)
        {
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
            {
                var message = $@"RtuRemoved: RTU {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            var nodeId = rtu.NodeId;
            _model.Traces.RemoveAll(t => t.RtuId == rtu.Id);
            _model.Rtus.Remove(rtu);
            _model.RemoveNodeWithAllHis(nodeId);
            return null;
        }

        public string AttachOtau(OtauAttached e)
        {
            Otau otau = _mapper.Map<Otau>(e);
            _model.Otaus.Add(otau);
            return null;
        }

        public string DetachOtau(OtauDetached e)
        {
            var otau =  _model.Otaus.FirstOrDefault(o => o.Id == e.Id);
            if (otau == null)
            {
                var message = $@"OtauDetached: OTAU {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            var rtu =  _model.Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                var message = $@"OtauDetached: RTU {e.RtuId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            rtu.FullPortCount -= otau.PortCount;
            _model.Otaus.Remove(otau);
            return null;
        }
    }
}