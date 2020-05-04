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
    public class TraceController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public TraceController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet("Information/{id}")]
        public async Task<TraceInformationDto> GetTraceInformation(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var traceInformationDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetTraceInformation(User.Identity.Name, traceGuid);
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
        public async Task<TraceStatisticsDto> GetTraceStatistics(string id, int pageNumber, int pageSize)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var traceStatisticsDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetTraceStatistics(User.Identity.Name, traceGuid, pageNumber, pageSize);
                _logFile.AppendLine(traceStatisticsDto == null
                    ? "Failed to get trace's statistics"
                    : $"trace has {traceStatisticsDto.BaseRefs.Count} refs and {traceStatisticsDto.MeasFullCount} measurements");
                return traceStatisticsDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetTraceStatistics: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("State/{id}")]
        public async Task<TraceStateDto> GetTraceState(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var traceStateDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetTraceState(User.Identity.Name, traceGuid);
                _logFile.AppendLine(traceStateDto == null
                    ? "Failed to get trace's state"
                    : $"trace state is {traceStateDto.TraceState}");
                return traceStateDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetTraceState: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpGet("Assign-base-params/{id}")]
        public async Task<AssignBaseParamsDto> GetAssignBaseParams(string id)
        {
            try
            {
                _logFile.AppendLine($"trace id = {id}");
                var traceGuid = Guid.Parse(id);
                var assignBaseParamsDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetAssignBaseParams(User.Identity.Name, traceGuid);
                _logFile.AppendLine(assignBaseParamsDto == null
                    ? "Failed to get trace's statistics"
                    : $"trace has RTU-title {assignBaseParamsDto.RtuTitle}");
                return assignBaseParamsDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"GetTraceStatistics: {e.Message}");
            }
            return null;
        }

        [Authorize]
        [HttpPost("Assign-base-refs")]
        public async Task<BaseRefAssignedDto> PostBaseRefs()
        {
            try
            {
                var dtoString = HttpContext.Request.Form["dto"];
                _logFile.AppendLine($"PostBaseRefs: {dtoString}");
                var inputDto = JsonConvert.DeserializeObject<AssignBaseRefDtoWithFiles>(dtoString);
                var dto = Map(inputDto);
                foreach (var file in HttpContext.Request.Form.Files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var tt = file.Name;
                        var fl = Enum.TryParse(tt, out BaseRefType baseRefType);
                        if (!fl) continue;
                        var baseRef = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == baseRefType);
                        if (baseRef == null) continue;
                        await file.CopyToAsync(memoryStream);
                        baseRef.SorBytes = memoryStream.ToArray();
                        baseRef.Id = Guid.NewGuid();
                        baseRef.UserName = User.Identity.Name;
                    }
                }

                var baseRefAssignedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .AssignBaseRefAsync(dto);
                _logFile.AppendLine($"PostBaseRefs: {baseRefAssignedDto.ReturnCode}");
                return baseRefAssignedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"PostBaseRefs: {e.Message}");
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = e.Message };
            }
        }

        private AssignBaseRefsDto Map(AssignBaseRefDtoWithFiles dto)
        {
            var result = new AssignBaseRefsDto
            {
                ClientIp = GetRemoteAddress(),
                Username = User.Identity.Name,
                RtuId = dto.RtuId,
                RtuMaker = dto.RtuMaker,
                OtdrId = dto.OtdrId,
                TraceId = dto.TraceId,
                OtauPortDto = dto.OtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };
            foreach (var baseRefFile in dto.BaseRefs)
            {
                result.BaseRefs.Add(new BaseRefDto() { BaseRefType = baseRefFile.Type });
            }
            return result;
        }
    }

}