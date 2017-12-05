using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MonitoringResultsManager
    {
        private readonly IMyLog _logFile;


        public MonitoringResultsManager(IMyLog logFile)
        {
            _logFile = logFile;
        }
        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            _logFile.AppendLine($"step on RTU {monitoringStep.RtuId.First6()}");
            return true;
        }

        public async Task<bool> ProcessMonitoringResult(MonitoringResultDto result)
        {
            var isTraceStateChanged = IsTraceStateChanged(result);

            var sorFileId = await SaveMeasurementInDb(result, isTraceStateChanged);
            if (sorFileId == -1)
                return false;

            if (isTraceStateChanged)
            {
                await SaveOpticalEventInDb(result, sorFileId);
                await SendMoniresultToClients(result);
            }

            return true;
        }

        private async Task<bool> SendMoniresultToClients(MonitoringResultDto result)
        {
            return true;
        }
        private async Task<bool> SaveOpticalEventInDb(MonitoringResultDto result, int measurementId)
        {
            try
            {
                var dbContext = new MySqlContext();
                dbContext.OpticalEvents.Add(new OpticalEvent()
                {
                    RtuId = result.RtuId,
                    TraceId = result.PortWithTrace.TraceId,
                    EventRegistrationTimestamp = result.TimeStamp,
                    BaseRefType = result.BaseRefType,
                    TraceState = result.TraceState,
                    EventStatus = result.TraceState == FiberState.Ok || result.BaseRefType == BaseRefType.Fast ? EventStatus.NotAnAccident : EventStatus.Unprocessed,
                    StatusChangedTimestamp = result.TimeStamp,
                    StatusChangedByUser = "",
                    Comment = "",
                    SorFileId = measurementId,
                });
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveOpticalEventInDb " + e.Message);
                return false;
            }
        }

        private bool IsTraceStateChanged(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new MySqlContext();
                var lastEventOnTrace = dbContext.OpticalEvents.Where(ev => ev.TraceId == result.PortWithTrace.TraceId).ToList()
                    .LastOrDefault();
                if (lastEventOnTrace == null)
                {
                    _logFile.AppendLine($"First event on trace {result.PortWithTrace.TraceId.First6()} - Save.");
                    return true;
                }
                if (lastEventOnTrace.TraceState != result.TraceState)
                {
                    _logFile.AppendLine($"State of trace {result.PortWithTrace.TraceId.First6()} changed - Save.");
                    return true;
                }
                if (lastEventOnTrace.BaseRefType != result.BaseRefType)
                {
                    _logFile.AppendLine($"Confirmation of accident on trace {result.PortWithTrace.TraceId.First6()} - Save.");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("IsTraceStateChanged " + e.Message);
                return false;
            }
        }

        private async Task<int> SaveMeasurementInDb(MonitoringResultDto result, bool isTraceStateChanged)
        {
            try
            {
                var dbContext = new MySqlContext();

                var sorFile = new SorFile() { SorBytes = result.SorData };
                dbContext.SorFiles.Add(sorFile);
                await dbContext.SaveChangesAsync();

                var sorFileId = sorFile.Id;

                dbContext.Measurements.Add(new Measurement()
                {
                    SorFileId = sorFileId,
                    RtuId = result.RtuId,
                    TraceId = result.PortWithTrace.TraceId,
                    BaseRefType = result.BaseRefType,
                    TraceState = result.TraceState,
                    IsOpticalEvent = isTraceStateChanged,
                    Timestamp = result.TimeStamp,
                });
                await dbContext.SaveChangesAsync();
                return sorFileId;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementInDb " + e.Message);
                return -1;
            }
        }
    }
}