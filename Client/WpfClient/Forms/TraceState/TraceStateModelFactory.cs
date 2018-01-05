using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelFactory
    {
        private readonly ReadModel _readModel;

        public TraceStateModelFactory(ReadModel readModel)
        {
            _readModel = readModel;
        }

        // TraceLeaf
        // Trace statistics
        // Monitoring result notification
        public TraceStateModel CreateModel(Measurement measurement, byte[] sorBytes)
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
                Comment = measurement.Comment
            };
            if (model.TraceState != FiberState.Ok)
                model.Accidents = PrepareAccidents(sorBytes);
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
            if (model.TraceState != FiberState.Ok)
                model.Accidents = PrepareAccidents(sorBytes);
            return model;
        }

        private List<AccidentLineModel> PrepareAccidents(byte[] sorBytes)
        {
            var lines = new List<AccidentLineModel>();
            if (sorBytes != null)
                lines.Add(new AccidentLineModel());
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