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
            _logger.LogInformation("MiscController CheckApi");
            return "Fibertest dotnet 6.0 RTU HTTP controller greets you!";
        }
    }
}
