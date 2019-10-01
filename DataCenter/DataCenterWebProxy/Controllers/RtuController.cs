using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.DataCenterWebProxy
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

        // GET: api/Rtu
        [HttpGet]
        public async Task<IEnumerable<RtuDto>> Get()
        {
            var rtuList = await _webProxy2DWcfManager.GetRtuList();
            _logFile.AppendLine(rtuList == null
                ? "Failed to get RTU list"
                : $"RTU list contains {rtuList.Count} items");
            return rtuList;
        }

        // GET: api/Rtu/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Rtu
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Rtu/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
