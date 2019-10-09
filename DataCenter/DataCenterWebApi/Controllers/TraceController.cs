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
    public class TraceController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebProxy2DWcfManager _webProxy2DWcfManager;

        public TraceController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webProxy2DWcfManager = new WebProxy2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebProxy);
            _webProxy2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }
        
        [HttpGet]
        public async Task<IEnumerable<TraceDto>> Get()
        {
            var traceList = await _webProxy2DWcfManager.GetTraceList();
            _logFile.AppendLine(traceList == null
                ? "Failed to get trace list"
                : $"trace list contains {traceList.Count} items");
            return traceList;
        }

    }
}