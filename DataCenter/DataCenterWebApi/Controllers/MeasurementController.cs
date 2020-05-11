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
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public MeasurementController(IniFile iniFile, IMyLog logFile)
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
                dto.ClientIp = GetRemoteAddress();
                dto.StatusChangedTimestamp = DateTime.Now;
                dto.StatusChangedByUser = User.Identity.Name;
                var result = await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .UpdateMeasurement(User.Identity.Name, dto);
                return result == null
                    ? new RequestAnswer() {ReturnCode = ReturnCode.Ok}
                    : new RequestAnswer() {ReturnCode = ReturnCode.Error, ErrorMessage = result};
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"UpdateMeasurement: {e.Message}");
                return new RequestAnswer() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
            }
        }
    }
}