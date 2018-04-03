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
    }
}