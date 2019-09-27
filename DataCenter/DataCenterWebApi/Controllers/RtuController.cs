using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class RtuController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public RtuController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }
        
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var rtuList = await _webProxy2DWcfManager.GetRtuList();
            _logFile.AppendLine(rtuList == null
                ? "Failed to get RTU list"
                : $"RTU list contains {rtuList.Count} items");
            return rtuList;
        }

        [HttpPost]
        public void Post([FromBody] Question question)
        {

        }
    }
}