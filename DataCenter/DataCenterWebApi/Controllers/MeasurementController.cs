using System;
using System.IO;
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
    public class MeasurementController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public MeasurementController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" || ip1 == "127.0.0.1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpPost("Update")]
        public async Task<RequestAnswer> UpdateMeasurement()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<UpdateMeasurementDto>(body);
                if (dto == null)
                    return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = "Failed to deserialize json" };

                dto.ClientIp = GetRemoteAddress();
                dto.StatusChangedTimestamp = DateTime.Now;
                dto.StatusChangedByUser = User.Identity.Name;
                var result = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .UpdateMeasurement(User.Identity.Name, dto);
                return result == null
                    ? new RequestAnswer() { ReturnCode = ReturnCode.Ok }
                    : new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = result };
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"UpdateMeasurement: {e.Message}");
                return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
            }
        }

        [Authorize]
        [HttpPost("Start-measurement-client")]
        public async Task<ClientMeasurementStartedDto> StartMeasurementClient()
        {
            _logFile.AppendLine($"MeasurementClient request from {GetRemoteAddress()}");

            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine($"{body}");

                var dto = JsonConvert.DeserializeObject<DoClientMeasurementDto>(body);
                var clientMeasurementStartedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .DoClientMeasurementAsync(dto);
                return clientMeasurementStartedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"StartMeasurementClient: {e.Message}");
                return new ClientMeasurementStartedDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.MeasurementPreparationError };
            }
        }

        [Authorize]
        [HttpGet("Get-measurement-client-result")]
        public async Task<ClientMeasurementVeexResultDto> GetVeexMeasurementClientResult(string rtuId, string veexMeasurementId)
        {
            _logFile.AppendLine($"Get Veex MeasurementClient result request from {GetRemoteAddress()}");

            try
            {
                var getDto = new GetClientMeasurementDto() { RtuId = Guid.Parse(rtuId), VeexMeasurementId = veexMeasurementId };

                var clientMeasurementStartedDto = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetClientMeasurementAsync(getDto);
                return clientMeasurementStartedDto;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"StartMeasurementClient: {e.Message}");
                return new ClientMeasurementVeexResultDto() { ErrorMessage = e.Message, ReturnCode = ReturnCode.Error };
            }
        }

        [Authorize]
        [HttpPost("Out-of-turn-measurement")]
        public async Task<RequestAnswer> OutOfTurnPreciseMeasurement()
        {
            _logFile.AppendLine($"OutOfTurnPreciseMeasurement request from {GetRemoteAddress()}");

            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                var dto = JsonConvert.DeserializeObject<DoOutOfTurnPreciseMeasurementDto>(body);
                var outOfTurnMeasurementStarted = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .DoOutOfTurnPreciseMeasurementAsync(dto);
                return outOfTurnMeasurementStarted;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"OutOfTurnPreciseMeasurement: {e.Message}");
                return new RequestAnswer() { ErrorMessage = e.Message, ReturnCode = ReturnCode.MeasurementPreparationError };
            }
        }
    }
}