using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelFactory
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly C2DWcfManager _c2DWcfManager;

        public TraceStateModelFactory(IMyLog logFile, ReadModel readModel, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        // TraceLeaf
        public async Task<TraceStateModel> Create(Guid traceId)
        {
            Measurement measurement = await GetLastTraceMeasurement(traceId);
            return CreateVm(measurement);
        }

        // Trace statistics
        // Server
        public TraceStateModel CreateVm(Measurement measurement)
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
                model.Accidents = PrepareAccidents();
            return model;
        }

        // Optical events
        public TraceStateModel CreateVm(OpticalEventModel opticalEventModel)
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
                model.Accidents = PrepareAccidents();
            return model;
        }

        private List<AccidentLineModel> PrepareAccidents()
        {
            return new List<AccidentLineModel>();
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

        private  async Task<Measurement> GetLastTraceMeasurement(Guid traceId)
        {
            var measurement = await _c2DWcfManager.GetLastMeasurementForTrace(traceId);
            if (measurement == null)
            {
                _logFile.AppendLine($@"Cannot get last measurement for trace {traceId.First6()}");
                return null;
            }
            else
            {
                _logFile.AppendLine($@"Last measurement for trace {traceId.First6()} received");
                return measurement;
            }
        }
    }
}