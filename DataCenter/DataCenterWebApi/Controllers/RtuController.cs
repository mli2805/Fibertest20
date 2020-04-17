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
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public RtuController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<RtuDto>> GetTree()
        {
            var tree = await _webC2DWcfManager
                .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                .GetTreeInJson(User.Identity.Name);
            _logFile.AppendLine(tree == null
                ? "Failed to get tree"
                : $"tree contains {tree.Length} symbols");
            var rtuList = (List<RtuDto>)JsonConvert.DeserializeObject(tree, JsonSerializerSettings);
            _logFile.AppendLine(rtuList == null
                ? "Failed to get RTU list"
                : $"RTU list contains {rtuList.Count} items");

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
                var rtuInformationDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuInformation(User.Identity.Name, rtuGuid);
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
                var rtuNetworkSettingsDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuNetworkSettings(User.Identity.Name, rtuGuid);
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
                var rtuStateDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuState(User.Identity.Name, rtuGuid);
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
        [HttpGet("Measurement-parameters/{id}")]
        public async Task<TreeOfAcceptableMeasParams> GetRtuAcceptableMeasParams(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuMeasParams = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuAcceptableMeasParams(User.Identity.Name, rtuGuid);
                _logFile.AppendLine(rtuMeasParams == null
                    ? "Failed to GetRtuAcceptableMeasParams"
                    : "RTU acceptable meas params ");
                return rtuMeasParams;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetRtuAcceptableMeasParams: {e.Message}");
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
                var rtuMonitoringSettingsDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuMonitoringSettings(User.Identity.Name, rtuGuid);
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
                var monitoringSettingsAppliedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, "webproxy", GetRemoteAddress())
                    .ApplyMonitoringSettingsAsync(applyDto);
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
                RtuMaker = dto.RtuMaker,
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

        [Authorize]
        [HttpPost("Start-monitoring/{id}")]
        public async Task<MonitoringSettingsAppliedDto> StartMonitoring(string id)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {id}");
                var rtuGuid = Guid.Parse(id);
                var rtuMonitoringSettingsDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "webproxy", GetRemoteAddress())
                    .GetRtuMonitoringSettings(User.Identity.Name, rtuGuid);
                rtuMonitoringSettingsDto.MonitoringMode = MonitoringState.On;
                var applyDto = Map(rtuGuid, rtuMonitoringSettingsDto);
                var monitoringSettingsAppliedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, "webproxy", GetRemoteAddress())
                    .ApplyMonitoringSettingsAsync(applyDto);
                _logFile.AppendLine($"StartMonitoring: {monitoringSettingsAppliedDto.ReturnCode.ToString()}");
                return monitoringSettingsAppliedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"StartMonitoring: {e.Message}");
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError, ExceptionMessage = e.Message };
            }
        }

        [Authorize]
        [HttpPost("Stop-monitoring/{id}")]
        public async Task<bool> StopMonitoring(string id)
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
                _logFile.AppendLine("body: " + body);
                var str = JsonConvert.DeserializeObject<string>(body);
                var rtuMaker = (RtuMaker)Enum.Parse(typeof(RtuMaker), str);
                var dto = new StopMonitoringDto() { RtuId = rtuGuid, RtuMaker = rtuMaker };
                var isStopped = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, "webproxy", GetRemoteAddress())
                    .StopMonitoringAsync(dto);
                return isStopped;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"StopMonitoring: {e.Message}");
                return false;
            }
        }


    }
}