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
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private WebProxy2DWcfManager _webProxy2DWcfManager;

        public RtuController(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            var wcfService = new WebProxy2DWcfManager(_iniFile, _logFile);
            var doubleAddress = _iniFile.ReadDoubleAddress((int) TcpPorts.ServerListenToWebProxy);

            wcfService.SetServerAddresses(doubleAddress, "webProxy", "localhost");
            _webProxy2DWcfManager = wcfService;
        }

        // GET: api/Rtu
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var rtus = await _webProxy2DWcfManager.GetRtuList();
            return rtus;
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
