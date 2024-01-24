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
    public class TablesController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly string _localIpAddress;

        public TablesController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" || ip1 == "127.0.0.1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet("GetOpticalsPage")]
        public async Task<OpticalEventsRequestedDto> GetOpticalsPage(bool isCurrentEvents,
                   string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize)
        {
            var resultDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity!.Name, GetRemoteAddress())
                    .GetOpticalEventPortion(User.Identity!.Name, isCurrentEvents, filterRtu, filterTrace, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(resultDto == null
                ? "Failed to get optical event list"
                : $"Optical event list contains {resultDto.FullCount} items");

            return resultDto;
        }

        [Authorize]
        [HttpGet("GetNetworksPage")]
        public async Task<NetworkEventsRequestedDto> GetNetworksPage(bool isCurrentEvents,
                          string filterRtu, string sortOrder, int pageNumber, int pageSize)
        {
            var resultDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity!.Name, GetRemoteAddress())
                    .GetNetworkEventPortion(User.Identity!.Name, isCurrentEvents, filterRtu, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(resultDto == null
                ? "Failed to get optical event list"
                : $"Optical event list contains {resultDto.FullCount} items");

            return resultDto;
        }

        [Authorize]
        [HttpGet("GetBopsPage")]
        public async Task<BopEventsRequestedDto> GetBopsPage(bool isCurrentEvents,
                                string filterRtu, string sortOrder, int pageNumber, int pageSize)
        {
            var resultDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity!.Name, GetRemoteAddress())
                    .GetBopEventPortion(User.Identity!.Name, isCurrentEvents, filterRtu, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(resultDto == null
                ? "Failed to get bop event list"
                : $"Bop event list contains {resultDto.FullCount} items");

            return resultDto;
        }

        [Authorize]
        [HttpGet("GetStateAccidentPage")]
        public async Task<RtuAccidentsRequestedDto> GetStateAccidentPage(bool isCurrentEvents,
                                     string sortOrder, int pageNumber, int pageSize)
        {
            var resultDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity!.Name, GetRemoteAddress())
                    .GetStateAccidentPortion(User.Identity!.Name, isCurrentEvents, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(resultDto == null
                ? "Failed to RTU status accidents list"
                : $"RTU status accidents list contains {resultDto.FullCount} items");

            return resultDto;
        }

    }
}