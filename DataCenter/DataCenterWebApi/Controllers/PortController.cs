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

        public PortController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _desktopC2DWcfManager = new DesktopC2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            _desktopC2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");

            var da = (DoubleAddress)doubleAddress.Clone();
            da.Main.Port = (int)TcpPorts.ServerListenToCommonClient;
            if (da.HasReserveAddress)
                da.Reserve.Port = (int)TcpPorts.ServerListenToCommonClient;
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _commonC2DWcfManager.SetServerAddresses(da, "webClient", "localhost");
        }

        [Authorize]
        [HttpPost("Attach-trace")]
        public async Task<string> AttachTrace()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<AttachTrace>(body);

                var result = await _desktopC2DWcfManager.SendCommandAsObj(dto);
                return string.IsNullOrEmpty(result) ? null : result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"AttachTrace: {e.Message}");
                return e.Message;
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

                var result = await _commonC2DWcfManager.AttachOtauAsync(dto);
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
                var result = await _desktopC2DWcfManager.SendCommandAsObj(new DetachTrace() { TraceId = traceGuid });
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
                var otauDetachedDto = await _commonC2DWcfManager.DetachOtauAsync(detachOtauDto);
                return otauDetachedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"DetachOtau: {e.Message}");
                return new OtauDetachedDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.RtuDetachOtauError };
            }
        }

        [Authorize]
        [HttpPost("Measurement-client")]
        public async Task<ClientMeasurementStartedDto> MeasurementClient()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine("body: " + body);
                var dto = JsonConvert.DeserializeObject<DoClientMeasurementDto>(body);
                var clientMeasurementStartedDto = await _commonC2DWcfManager.DoClientMeasurementAsync(dto);
                return clientMeasurementStartedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"MeasurementClient: {e.Message}");
                return new ClientMeasurementStartedDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.MeasurementPreparationError };
            }
        }


    }
}