using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public class MeasurementEventOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IModel _model;
        private readonly AccidentsOnTraceApplierToReadModel _accidentsOnTraceApplierToReadModel;

        public MeasurementEventOnModelExecutor(IModel model, AccidentsOnTraceApplierToReadModel accidentsOnTraceApplierToReadModel)
        {
            _model = model;
            _accidentsOnTraceApplierToReadModel = accidentsOnTraceApplierToReadModel;
        }

        public string AddMeasurement(MeasurementAdded e)
        {
            _model.Measurements.Add(_mapper.Map<Measurement>(e));
            _accidentsOnTraceApplierToReadModel.ShowMonitoringResult(e);
            return null;
        }

        public string UpdateMeasurement(MeasurementUpdated e)
        {
            var destination = _model.Measurements.First(f => f.SorFileId == e.SorFileId);
            _mapper.Map(e, destination);
            return null;
        }
    }
}