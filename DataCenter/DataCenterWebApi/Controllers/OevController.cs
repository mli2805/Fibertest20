using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class OevController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly string _localIpAddress;

        public OevController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet("GetPage")]
        public async Task<OpticalEventsRequestedDto> GetPage(bool isCurrentEvents,
                   string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize)
        {
            var resultDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetOpticalEventPortion(User.Identity.Name, isCurrentEvents, filterRtu, filterTrace, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(resultDto == null
                ? "Failed to get optical event list"
                : $"Optical event list contains {resultDto.FullCount} items");

            return resultDto;
        }

    }
}