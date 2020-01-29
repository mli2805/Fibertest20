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
    [ApiController]
    public class OevController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;

        public OevController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webC2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

      
        [Authorize]
        [HttpGet("GetPage")]
        public async Task<IEnumerable<OpticalEventDto>> GetPage(bool isCurrentEvents, string filterRtu, string filterTrace, string sortOrder, int pageNumber, int pageSize)
        {
            var opticalEventList = await _webC2DWcfManager.
                GetOpticalEventList(User.Identity.Name, isCurrentEvents, filterRtu, filterTrace, sortOrder, pageNumber, pageSize);
            _logFile.AppendLine(opticalEventList == null
                ? "Failed to get optical event list"
                : $"Optical event list contains {opticalEventList.Count} items");

            return opticalEventList;
        }

    }
}