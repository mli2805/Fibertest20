using System;
using System.Collections.Generic;
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
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public TraceController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IEnumerable<TraceDto>> GetAllTraces()
        {
            var traceList = await _webProxy2DWcfManager.GetTraceList();
            _logFile.AppendLine(traceList == null
                ? "Failed to get trace list"
                : $"trace list contains {traceList.Count} items");
            return traceList;
        }


        [Authorize]
        [HttpGet("Information/{id}")]
        public async Task<TraceInformationDto> GetTraceInformation(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                _logFile.AppendLine($"trace Guid = {traceGuid}");
                var traceInformationDto = await _webProxy2DWcfManager.GetTraceInformation(traceGuid);
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
                _logFile.AppendLine($"trace Guid = {traceGuid}");
                var traceStatisticsDto = await _webProxy2DWcfManager.GetTraceStatistics(traceGuid);
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