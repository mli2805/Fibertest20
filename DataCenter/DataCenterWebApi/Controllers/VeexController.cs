using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            var notification = JsonConvert.DeserializeObject<VeexNotification>(body);
            _logFile.AppendLine($"Notification {notification.type} from VeEX RTU, {notification.events.Count} event(s) received");
            foreach (var notificationEvent in notification.events)
            {
                _logFile.AppendLine($"test {notificationEvent.data.testId} - {notificationEvent.type} at {notificationEvent.time}");
            }
            Response.StatusCode = 200;
        }
    }
}