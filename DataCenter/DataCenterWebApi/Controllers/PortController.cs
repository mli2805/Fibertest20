using System;
using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    public class PortController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForDesktopWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public PortController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _desktopC2DWcfManager = new DesktopC2DWcfManager(iniFile, logFile);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _doubleAddressForDesktopWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpPost("Attach-trace")]
        public async Task<RequestAnswer> AttachTrace()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<AttachTraceDto>(body);

                return await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .AttachTraceAndSendBaseRefs(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"AttachTrace: {e.Message}");
                return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
            }
        }

        [Authorize]
        [HttpPost("Attach-otau")]
        public async Task<OtauAttachedDto> AttachOtau()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<AttachOtauDto>(body);
                dto.OtauId = Guid.NewGuid();

                var result = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .AttachOtauAsync(dto);
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"AttachOtau: {e.Message}");
                return new OtauAttachedDto() { ReturnCode = ReturnCode.RtuAttachOtauError, ErrorMessage = e.Message };
            }
        }

        [Authorize]
        [HttpPost("Detach-trace/{id}")]
        public async Task<string> DetachTrace(string id)
        {
            try
            {
                var traceGuid = Guid.Parse(id);
                var result = await _desktopC2DWcfManager
                    .SetServerAddresses(_doubleAddressForDesktopWcfManager, User.Identity.Name, GetRemoteAddress())
                    .SendCommandAsObj(new DetachTrace() { TraceId = traceGuid });
                return string.IsNullOrEmpty(result) ? null : result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"DetachTrace: {e.Message}");
                return e.Message;
            }
        }

        [Authorize]
        [HttpPost("Detach-otau")]
        public async Task<OtauDetachedDto> DetachOtau()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine("body: " + body);
                var detachOtauDto = JsonConvert.DeserializeObject<DetachOtauDto>(body);
                var otauDetachedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .DetachOtauAsync(detachOtauDto);
                return otauDetachedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"DetachOtau: {e.Message}");
                return new OtauDetachedDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.RtuDetachOtauError };
            }
        }


    }
}