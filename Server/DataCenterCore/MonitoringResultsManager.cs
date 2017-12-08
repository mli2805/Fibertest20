using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class MonitoringResultsManager
    {
        private readonly IMyLog _logFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly D2CWcfManager _d2CWcfManager;


        public MonitoringResultsManager(IMyLog logFile, ClientRegistrationManager clientRegistrationManager, D2CWcfManager d2CWcfManager)
        {
            _logFile = logFile;
            _clientRegistrationManager = clientRegistrationManager;
            _d2CWcfManager = d2CWcfManager;
        }

        public async Task<bool> ProcessMonitoringResult(MonitoringResultDto result)
        {
            var measurement = await SaveMeasurementInDb(result);
            if (measurement == null)
                return false;

            await SendMoniresultToClients(measurement);

            return true;
        }

        private async Task<int> SendMoniresultToClients(Measurement measurement)
        {
            var addresses = await _clientRegistrationManager.GetClientsAddresses();
            if (addresses == null)
                return 0;
            _d2CWcfManager.SetClientsAddresses(addresses);
            return await _d2CWcfManager.NotifyAboutMonitoringResult(measurement);
        }

        private bool IsEvent(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new MySqlContext();
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
                return EventStatus.NotAnAccident;
            return EventStatus.Unprocessed;
        }

        private async Task<Measurement> SaveMeasurementInDb(MonitoringResultDto result)
        {
            try
            {
                var dbContext = new MySqlContext();

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
                _logFile.AppendLine("SaveMeasurementInDb " + e.Message);
                return null;
            }
        }
    }
}