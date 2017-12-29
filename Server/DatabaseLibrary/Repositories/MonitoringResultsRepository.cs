using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class MonitoringResultsRepository
    {
        private readonly IMyLog _logFile;

        public MonitoringResultsRepository(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<Measurement> SaveMonitoringResultAsync(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new FtDbContext();

                var sorFile = new SorFile() { SorBytes = result.SorData };
                dbContext.SorFiles.Add(sorFile);
                await dbContext.SaveChangesAsync();

                var sorFileId = sorFile.Id;

                var measurement = new Measurement()
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

                    SorFileId = sorFileId,
                };
                dbContext.Measurements.Add(measurement);
                await dbContext.SaveChangesAsync();
                return measurement;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementAsync: " + e.Message);
                return null;
            }
        }

        private bool IsEvent(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new FtDbContext();
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