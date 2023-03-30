using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelFactory
    {
        private readonly Model _readModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly CurrentGis _currentGis;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly ReflectogramManager _reflectogramManager;

        public TraceStateModelFactory(Model readModel, AccidentLineModelFactory accidentLineModelFactory,
            CurrentGis currentGis, CurrentDatacenterParameters currentDatacenterParameters, ReflectogramManager reflectogramManager)
        {
            _readModel = readModel;
            _accidentLineModelFactory = accidentLineModelFactory;
            _currentGis = currentGis;
            _currentDatacenterParameters = currentDatacenterParameters;
            _reflectogramManager = reflectogramManager;
        }

        // TraceLeaf
        // Trace statistics
        // Monitoring result notification
        public async Task<TraceStateModel> CreateModel(Measurement measurement, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(measurement.TraceId),
                TraceId = measurement.TraceId,
                Trace = _readModel.Traces.First(t => t.TraceId == measurement.TraceId),
                TraceState = measurement.TraceState,
                BaseRefType = measurement.BaseRefType,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                RegistrationTimestamp = measurement.EventRegistrationTimestamp,
                SorFileId = measurement.SorFileId,
                EventStatus = measurement.EventStatus,
                Comment = measurement.Comment,

                IsLastStateForThisTrace = isLastStateForThisTrace,
                IsLastAccidentForThisTrace = isLastAccidentForThisTrace,
            };
            if (model.TraceState != FiberState.Ok)
                if (measurement.Accidents.Count == 0)
                    model.Accidents = await GetTotalFiberLossAccident(measurement);
                else
                    model.Accidents = PrepareAccidents(measurement.Accidents);
            return model;
        }
        
        /// <summary>
        ///  for old measurements (DC version less than 1316)
        /// 
        ///  when Total fiber loss exceeded AccidentLineModel was not created
        /// </summary>
        /// <returns></returns>
        private async Task<List<AccidentLineModel>> GetTotalFiberLossAccident(Measurement measurement)
        {
            var sorBytes = await _reflectogramManager.GetSorBytes(measurement.SorFileId);
            var sorData = SorData.FromBytes(sorBytes);

            var baseSorData = sorData.GetBase();
            sorData.RftsParameters = baseSorData.RftsParameters;

            var rftsEventsBlock = sorData.GetRftsEventsBlockForEveryLevel().LastOrDefault();

           

            var result = new List<AccidentLineModel>();
            if (rftsEventsBlock == null)
                return result;

            if ((rftsEventsBlock.EELD.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == measurement.TraceId);
                var leftNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds.First());
                var rightNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds.Last());

                var a2 = new AccidentOnTraceV2()
                {
                    AccidentSeriousness = rftsEventsBlock.LevelName.ConvertToFiberState(),
                    OpticalTypeOfAccident = OpticalAccidentType.TotalLoss,
                    IsAccidentInOldEvent = true,
                    Left = new AccidentNeighbour() { Title = leftNode.Title },
                    Right = new AccidentNeighbour() { Title = rightNode.Title },
                };
                var aaa = _accidentLineModelFactory
                    .Create(a2, 111, _currentGis.IsGisOn, _currentGis.GpsInputMode);
                result.Add(aaa);
            }

            return result;
        }

        // Optical events
        public TraceStateModel CreateModel(OpticalEventModel opticalEventModel, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            try
            {
                TraceStateModel model = new TraceStateModel
                {
                    Header = PrepareHeader(opticalEventModel.TraceId),
                    TraceId = opticalEventModel.TraceId,
                    Trace = _readModel.Traces.First(t => t.TraceId == opticalEventModel.TraceId),
                    TraceState = opticalEventModel.TraceState,
                    BaseRefType = opticalEventModel.BaseRefType,
                    MeasurementTimestamp = opticalEventModel.MeasurementTimestamp,
                    RegistrationTimestamp = opticalEventModel.EventRegistrationTimestamp,
                    SorFileId = opticalEventModel.SorFileId,
                    EventStatus = opticalEventModel.EventStatus,
                    Accidents = PrepareAccidents(opticalEventModel.Accidents),
                    Comment = opticalEventModel.Comment,
                    IsLastStateForThisTrace = isLastStateForThisTrace,
                    IsLastAccidentForThisTrace = isLastAccidentForThisTrace
                };
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<AccidentLineModel> PrepareAccidents(List<AccidentOnTraceV2> accidents)
        {
            var lines = new List<AccidentLineModel>();
            for (var i = 0; i < accidents.Count; i++)
            {
                lines.Add(_accidentLineModelFactory.Create(accidents[i], i + 1, _currentGis.IsGisOn, _currentGis.GpsInputMode));
            }
            return lines;
        }

        private TraceStateModelHeader PrepareHeader(Guid traceId)
        {
            var result = new TraceStateModelHeader();
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return result;

            result.TraceTitle = trace.Title;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            result.RtuPosition = _readModel.Nodes.FirstOrDefault(n => n.NodeId == rtu?.NodeId)?.Position;
            result.RtuTitle = rtu?.Title;
            result.PortTitle = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.MainCharonPort}-{trace.OtauPort.OpticalPort}";
            result.RtuSoftwareVersion = rtu?.Version;

            result.ServerTitle = _currentDatacenterParameters.ServerTitle;
            return result;
        }
    }
}