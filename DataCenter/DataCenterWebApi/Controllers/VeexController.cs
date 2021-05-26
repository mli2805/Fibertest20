using System;
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
        public async Task Notify(Guid rtuId)
        {
            _logFile.AppendLine(rtuId.ToString());
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var notification = JsonConvert.DeserializeObject<VeexNotification>(body);
            _logFile.AppendLine($"Notification {notification.type} from VeEX RTU {rtuId.First6()}, {notification.events.Count} event(s) received");
            foreach (var notificationEvent in notification.events)
            {
                var localTime = TimeZoneInfo.ConvertTime(notificationEvent.time, TimeZoneInfo.Local);
                _logFile.AppendLine($"test {notificationEvent.data.testId} - {notificationEvent.type} at {localTime}");
            }
            Response.StatusCode = 200;
        }
    }
}