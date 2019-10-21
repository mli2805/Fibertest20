using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class OevController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public OevController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [HttpGet]
        public async Task<IEnumerable<OpticalEventDto>> Get()
        {
            var opticalEventList = await _webProxy2DWcfManager.GetOpticalEventList();
            _logFile.AppendLine(opticalEventList == null
                ? "Failed to get optical event list"
                : $"Optical event list contains {opticalEventList.Count} items");

            return opticalEventList;
        }

    }
}