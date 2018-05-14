using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public class MeasurementEventOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly Model _model;
        private readonly AccidentsOnTraceToModelApplier _accidentsOnTraceToModelApplier;

        public MeasurementEventOnModelExecutor(Model model, AccidentsOnTraceToModelApplier accidentsOnTraceToModelApplier)
        {
            _model = model;
            _accidentsOnTraceToModelApplier = accidentsOnTraceToModelApplier;
        }

        public string AddMeasurement(MeasurementAdded e)
        {
            _model.Measurements.Add(Mapper.Map<Measurement>(e));
            _accidentsOnTraceToModelApplier.ShowMonitoringResult(e);
            return null;
        }

        public string UpdateMeasurement(MeasurementUpdated e)
        {
            var destination = _model.Measurements.First(f => f.SorFileId == e.SorFileId);
            Mapper.Map(e, destination);
            return null;
        }

        public string AddNetworkEvent(NetworkEventAdded e)
        {
            _model.NetworkEvents.Add(Mapper.Map<NetworkEvent>(e));
            var rtu = _model.Rtus.First(r => r.Id == e.RtuId);
            rtu.MainChannelState = e.MainChannelState;
            rtu.ReserveChannelState = e.ReserveChannelState;
            return null;
        }
        public string AddBopNetworkEvent(BopNetworkEventAdded e)
        {
            _model.BopNetworkEvents.Add(Mapper.Map<BopNetworkEvent>(e));
            var otau = _model.Otaus.First(o=>o.NetAddress.Ip4Address == e.OtauIp);
            otau.IsOk = e.IsOk;
            return null;
        }

    }
}