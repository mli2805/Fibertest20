using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class RtuController : ControllerBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;
        private readonly C2RWcfManager _c2RWcfManager;

        public RtuController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
            var da = (DoubleAddress)doubleAddress.Clone();
            da.Main.Port = (int)TcpPorts.ServerListenToC2R;
            if (da.HasReserveAddress) da.Reserve.Port = (int)TcpPorts.ServerListenToC2R;    _c2RWcfManager = new C2RWcfManager(iniFile, logFile);
            _c2RWcfManager.SetServerAddresses(da, "webClient", "localhost");
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<RtuDto>> GetTree()
        {
            var tree = await _webProxy2DWcfManager.GetTreeInJson(User.Identity.Name);
            _logFile.AppendLine(tree == null
                ? "Failed to get tree"
                : $"tree contains {tree.Length} symbols");
            var rtuList = (List<RtuDto>)JsonConvert.DeserializeObject(tree, JsonSerializerSettings);
            _logFile.AppendLine(rtuList == null
                ? "Failed to get list"
                : $"list contains {rtuList.Count} items");

            return rtuList;
        }

        [Authorize]
        [HttpGet("Information/{id}")]
        public async Task<RtuInformationDto> GetRtuInformation(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuInformationDto = await _webProxy2DWcfManager.GetRtuInformation(User.Identity.Name, rtuGuid);
                _logFile.AppendLine(rtuInformationDto == null
                    ? "Failed to get RTU's information"
                    : "RTU information ");
                return rtuInformationDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuInformation: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("Network-settings/{id}")]
        public async Task<RtuNetworkSettingsDto> GetRtuNetworkSettings(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuNetworkSettingsDto = await _webProxy2DWcfManager.GetRtuNetworkSettings(User.Identity.Name, rtuGuid);
                _logFile.AppendLine(rtuNetworkSettingsDto == null
                    ? "Failed to get RTU's network-settings"
                    : "RTU Network-settings ");
                return rtuNetworkSettingsDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuNetworkSettings: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("State/{id}")]
        public async Task<RtuStateDto> GetRtuState(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuStateDto = await _webProxy2DWcfManager.GetRtuState(User.Identity.Name, rtuGuid);
                _logFile.AppendLine(rtuStateDto == null
                    ? "Failed to get RTU's state"
                    : "RTU state ");
                return rtuStateDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuState: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("Monitoring-settings/{id}")]
        public async Task<RtuMonitoringSettingsDto> GetRtuMonitoringSettings(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuMonitoringSettingsDto = await _webProxy2DWcfManager.GetRtuMonitoringSettings(User.Identity.Name, rtuGuid);
                _logFile.AppendLine(rtuMonitoringSettingsDto == null
                    ? "Failed to get RTU's Monitoring-settings"
                    : "RTU Monitoring-settings ");
                return rtuMonitoringSettingsDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuMonitoringSettings: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpPost("Monitoring-settings/{id}")]
        public async Task<MonitoringSettingsAppliedDto> PostRtuMonitoringSettings(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<RtuMonitoringSettingsDto>(body);
                var applyDto = Map(rtuGuid, dto);
                var monitoringSettingsAppliedDto = await _c2RWcfManager.ApplyMonitoringSettingsAsync(applyDto);
                _logFile.AppendLine($"PostRtuMonitoringSettings: {monitoringSettingsAppliedDto.ReturnCode.ToString()}");
                return monitoringSettingsAppliedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"PostRtuMonitoringSettings: {e.Message}");
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError, ExceptionMessage = e.Message };
            }
        }

        private ApplyMonitoringSettingsDto Map(Guid rtuId, RtuMonitoringSettingsDto dto)
        {
            var applyMonitoringSettingsDto = new ApplyMonitoringSettingsDto()
            {
                RtuId = rtuId,
                IsMonitoringOn = dto.MonitoringMode == MonitoringState.On,

                Timespans = new MonitoringTimespansDto()
                {
                    FastSave = dto.FastSave.GetTimeSpan(),
                    PreciseMeas = dto.PreciseMeas.GetTimeSpan(),
                    PreciseSave = dto.PreciseSave.GetTimeSpan(),
                },

                Ports = new List<PortWithTraceDto>(),
            };
            foreach (var line in dto.Lines.Where(l => l.PortMonitoringMode == PortMonitoringMode.On))
            {
                var traceGuid = Guid.Parse(line.TraceId);
                var portWithTraceDto = new PortWithTraceDto()
                {
                    TraceId = traceGuid,
                    OtauPort = line.OtauPortDto,
                };
                applyMonitoringSettingsDto.Ports.Add(portWithTraceDto);
            }

            return applyMonitoringSettingsDto;
        }

    }
}