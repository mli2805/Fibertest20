using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class VeexController : ControllerBase
    {
        private readonly IMyLog _logFile;

        public VeexController(IMyLog logFile)
        {
            _logFile = logFile;
        }

        [HttpPost("Notify")]
        public async Task Notify()
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            _logFile.AppendLine(body);
            dynamic notification = JObject.Parse(body);
            _logFile.AppendLine($"Notification from VeEX RTU {(string)notification.rtuId} received");
            Response.StatusCode = 200;
        }
    }
}