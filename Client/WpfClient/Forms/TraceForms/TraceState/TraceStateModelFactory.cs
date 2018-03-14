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
        private readonly ReadModel _readModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;

        public TraceStateModelFactory(ReadModel readModel, AccidentLineModelFactory accidentLineModelFactory)
        {
            _readModel = readModel;
            _accidentLineModelFactory = accidentLineModelFactory;
        }

        // TraceLeaf
        // Trace statistics
        // Monitoring result notification
        public TraceStateModel CreateModel(Measurement measurement)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(measurement.TraceId),
                TraceId = measurement.TraceId,
                TraceState = measurement.TraceState,
                BaseRefType = measurement.BaseRefType,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                SorFileId = measurement.SorFileId,
                EventStatus = measurement.EventStatus,
                Comment = measurement.Comment,
            };
            if (model.TraceState != FiberState.Ok)
                model.Accidents = PrepareAccidents(measurement.Accidents);
            return model;
        }

        // Optical events
        public TraceStateModel CreateModel(OpticalEventModel opticalEventModel, byte[] sorBytes)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(opticalEventModel.TraceId),
                TraceId = opticalEventModel.TraceId,
                TraceState = opticalEventModel.TraceState,
                BaseRefType = opticalEventModel.BaseRefType,
                MeasurementTimestamp = opticalEventModel.MeasurementTimestamp,
                SorFileId = opticalEventModel.SorFileId,
                EventStatus = opticalEventModel.EventStatus,
                Comment = opticalEventModel.Comment
            };
//            if (model.TraceState != FiberState.Ok && model.TraceState != FiberState.NoFiber)
//                model.Accidents = PrepareAccidents(sorBytes, opticalEventModel.TraceId);
            return model;
        }

        private List<AccidentLineModel> PrepareAccidents(List<AccidentOnTrace> accidents)
        {
            var lines = new List<AccidentLineModel>();
            for (var i = 0; i < accidents.Count; i++)
            {
                lines.Add(_accidentLineModelFactory.Create(accidents[i], i+1));
            }
            return lines;
        }

        private TraceStateModelHeader PrepareHeader(Guid traceId)
        {
            var result = new TraceStateModelHeader();
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return result;

            result.TraceTitle = trace.Title;
            result.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            result.PortTitle = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";
            return result;
        }
    }
}