using System;
using System.Linq;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MeasurementFactory
    {
        private readonly ISettings _settings;
        private readonly IMyLog _logFile;

        public MeasurementFactory(ISettings settings, IMyLog logFile)
        {
            _settings = settings;
            _logFile = logFile;
        }

        public Measurement Create(MonitoringResultDto result)
        {
            return new Measurement()
            {
                MeasurementTimestamp = result.TimeStamp,
                EventRegistrationTimestamp = DateTime.Now,
                RtuId = result.RtuId,
                TraceId = result.PortWithTrace.TraceId,
                BaseRefType = result.BaseRefType,
                TraceState = result.TraceState,

                EventStatus = EvaluateStatus(result),
                StatusChangedTimestamp = DateTime.Now,
                StatusChangedByUser = "system",
                Comment = "",
            };
        }
        private bool IsEvent(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new FtDbContext(_settings.Options);
                var previousMeasurementOnTrace = dbContext.Measurements.Where(ev => ev.TraceId == result.PortWithTrace.TraceId).ToList()
                    .LastOrDefault();
                if (previousMeasurementOnTrace == null)
                {
                    _logFile.AppendLine($"First measurement on trace {result.PortWithTrace.TraceId.First6()} - event.");
                    return true;
                }
                if (previousMeasurementOnTrace.TraceState != result.TraceState)
                {
                    _logFile.AppendLine($"State of trace {result.PortWithTrace.TraceId.First6()} changed - event.");
                    return true;
                }
                if (previousMeasurementOnTrace.BaseRefType == BaseRefType.Fast
                    && previousMeasurementOnTrace.EventStatus > EventStatus.JustMeasurementNotAnEvent // fast measurement could be made 
                    // when monitoring mode is turned to Automatic 
                    // or it could be made by schedule
                    // but we are interested only in Events
                    && result.BaseRefType != BaseRefType.Fast // Precise or Additional
                    && result.TraceState != FiberState.Ok)
                {
                    _logFile.AppendLine($"Confirmation of accident on trace {result.PortWithTrace.TraceId.First6()} - event.");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("IsEvent " + e.Message);
                return false;
            }
        }

        private EventStatus EvaluateStatus(MonitoringResultDto result)
        {
            if (!IsEvent(result))
                return EventStatus.JustMeasurementNotAnEvent;
            if (result.TraceState == FiberState.Ok || result.BaseRefType == BaseRefType.Fast)
                return EventStatus.EventButNotAnAccident;
            return EventStatus.Unprocessed;
        }

    }
}