using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MonitoringResultsManager
    {
        private readonly IMyLog _logFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;


        public MonitoringResultsManager(IMyLog logFile, ClientRegistrationManager clientRegistrationManager)
        {
            _logFile = logFile;
            _clientRegistrationManager = clientRegistrationManager;
        }
        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            return true;
        }

        public async Task<bool> ProcessMonitoringResult(MonitoringResultDto result)
        {
            if (!await SaveMeasurementInDb(result))
                return false;

            if (IsTraceStateChanged(result))
            {
                await SaveOpticalEventInDb(result);
                await SendMoniresultToClients(result);
            }

            return true;
        }

        private async Task<bool> SendMoniresultToClients(MonitoringResultDto result)
        {
            return true;
        }
        private async Task<bool> SaveOpticalEventInDb(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new MySqlContext();
                dbContext.OpticalEvents.Add(new OpticalEvent()
                {
                    RtuId = result.RtuId,
                    TraceId = result.PortWithTrace.TraceId,
                    EventTimestamp = result.TimeStamp,
                    TraceState = result.TraceState,
                    EventStatus = EventStatus.Current,
                    StatusTimestamp = DateTime.Now,
                    StatusUserId = 0,
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
                return lastEventOnTrace?.TraceState != result.TraceState;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("IsTraceStateChanged " + e.Message);
                return false;
            }
        }
        private async Task<bool> SaveMeasurementInDb(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new MySqlContext();
                dbContext.Measurements.Add(new Measurement()
                {
                    MeasurementId = result.Id,
                    RtuId = result.RtuId,
                    TraceId = result.PortWithTrace.TraceId,
                    BaseRefType = result.BaseRefType,
                    TraceState = result.TraceState,
                    Timestamp = result.TimeStamp,
                });
                dbContext.SorFiles.Add(new SorFile()
                {
                    MeasurementId = result.Id,
                    SorBytes = result.SorData,
                });
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("SaveMeasurementInDb " + e.Message);
                return false;
            }
        }
    }
}