using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    public class TraceController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;

        public TraceController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _webC2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [Authorize]
        [HttpGet("Information/{id}")]
        public async Task<TraceInformationDto> GetTraceInformation(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var traceInformationDto = await _webC2DWcfManager.GetTraceInformation(User.Identity.Name, traceGuid);
                _logFile.AppendLine(traceInformationDto == null
                    ? "Failed to get trace's information"
                    : "Trace information ");
                return traceInformationDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetTraceInformation: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("Statistics/{id}")]
        public async Task<TraceStatisticsDto> GetTraceStatistics(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var traceStatisticsDto = await _webC2DWcfManager.GetTraceStatistics(User.Identity.Name, traceGuid);
                _logFile.AppendLine(traceStatisticsDto == null
                    ? "Failed to get trace's statistics"
                    : $"trace has {traceStatisticsDto.BaseRefs.Count} refs and {traceStatisticsDto.Measurements.Count} measurements");
                return traceStatisticsDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetTraceStatistics: {e.Message}");
            }
            return null;
        }

    }

}