using System;
using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    /// <summary>
    /// From DataCenter to WebApi (for WebClients)
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly IHubContext<SignalRHub> _hubContext;

        public ProxyController(IMyLog logFile, IHubContext<SignalRHub> _hubContext)
        {
            _logFile = logFile;
            this._hubContext = _hubContext;
        }

      
        [HttpPost("Client-measurement-done")]
        public async Task<bool> ClientMeasurementDone()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                var dto = JsonConvert.DeserializeObject<ClientMeasurementDoneDto>(body);
                _logFile.AppendLine($"ClientMeasurementDone for client: {dto.ClientIp}");
                await _hubContext.Clients.All.SendAsync("ClientMeasurementDone", body);
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"ClientMeasurementDone: {e.Message}");
                return false;
            }
        }  
        
        [HttpPost("Notify-monitoring-step")]
        public async Task<bool> NotifyMonitoringStep()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                await _hubContext.Clients.All.SendAsync("MonitoringStepNotified", body);
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"NotifyMeasStep: {e.Message}");
                return false;
            }
        }
    }
}