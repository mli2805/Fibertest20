using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelFactory
    {
        private readonly Model _readModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;

        public TraceStateModelFactory(Model readModel, AccidentLineModelFactory accidentLineModelFactory)
        {
            _readModel = readModel;
            _accidentLineModelFactory = accidentLineModelFactory;
        }

        // TraceLeaf
        // Trace statistics
        // Monitoring result notification
        public TraceStateModel CreateModel(Measurement measurement, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(measurement.TraceId),
                TraceId = measurement.TraceId,
                Trace = _readModel.Traces.First(t=>t.TraceId == measurement.TraceId),
                TraceState = measurement.TraceState,
                BaseRefType = measurement.BaseRefType,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                SorFileId = measurement.SorFileId,
                EventStatus = measurement.EventStatus,
                Comment = measurement.Comment,

                IsLastStateForThisTrace = isLastStateForThisTrace, 
                IsLastAccidentForThisTrace = isLastAccidentForThisTrace,
            };
            if (model.TraceState != FiberState.Ok)
                model.Accidents = PrepareAccidents(measurement.Accidents);
            return model;
        }

        // Optical events
        public TraceStateModel CreateModel(OpticalEventModel opticalEventModel, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(opticalEventModel.TraceId),
                TraceId = opticalEventModel.TraceId,
                Trace = _readModel.Traces.First(t=>t.TraceId == opticalEventModel.TraceId),
                TraceState = opticalEventModel.TraceState,
                BaseRefType = opticalEventModel.BaseRefType,
                MeasurementTimestamp = opticalEventModel.MeasurementTimestamp,
                SorFileId = opticalEventModel.SorFileId,
                EventStatus = opticalEventModel.EventStatus,
                Accidents = PrepareAccidents(opticalEventModel.Accidents),
                Comment = opticalEventModel.Comment,

                IsLastStateForThisTrace = isLastStateForThisTrace, 
                IsLastAccidentForThisTrace = isLastAccidentForThisTrace,
            };
            return model;
        }

        private List<AccidentLineModel> PrepareAccidents(List<AccidentOnTrace> accidents)
        {
            var lines = new List<AccidentLineModel>();
            for (var i = 0; i < accidents.Count; i++)
            {
                lines.Add(_accidentLineModelFactory.Create(accidents[i], i + 1));
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
               // : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";
                : $@"{trace.OtauPort.Serial}-{trace.OtauPort.OpticalPort}";
            return result;
        }
    }
}