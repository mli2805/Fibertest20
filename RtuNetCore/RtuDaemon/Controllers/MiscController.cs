using Iit.Fibertest.UtilsNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Iit.Fibertest.RtuDaemon
{
    [ApiController]
    [Route("[controller]")]
    public class MiscController : ControllerBase
    {
        private readonly ILogger<MiscController> _logger;

        public MiscController(ILogger<MiscController> logger)
        {
            _logger = logger;
        }

        [HttpGet("CheckApi")]
        public string CheckApi()
        {
            _logger.Info(Logs.RtuService, "MiscController CheckApi");
            return "Fibertest dotnet 8.0 RTU HTTP service greets you!" + Environment.NewLine + Environment.NewLine;
        }
    }
}
