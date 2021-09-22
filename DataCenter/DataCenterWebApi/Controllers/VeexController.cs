using System;
using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class VeexController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly WebC2DWcfManager _webVeexWcfManager;
        private readonly string _localIpAddress;

        public VeexController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _webVeexWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" || ip1 == "127.0.0.1" ? _localIpAddress : ip1;
        }

        // [HttpPost("Notify")]
        // public async Task Notify(Guid rtuId)
        // {
        //     string body;
        //     using (var reader = new StreamReader(Request.Body))
        //     {
        //         body = await reader.ReadToEndAsync();
        //     }
        //     var notification = JsonConvert.DeserializeObject<VeexNotification>(body);
        //     _logFile.AppendLine($"Notification from VeEX RTU {rtuId.First6()}, {notification.events.Count} event(s) received");
        //
        //     _ = await _webVeexWcfManager
        //         .SetServerAddresses(_doubleAddressForWebWcfManager, "VeexOtaus Controller", GetRemoteAddress())
        //         .MonitoringMeasurementDone(new VeexMeasurementDto(){RtuId = rtuId, VeexNotification =  notification});
        //
        //     Response.StatusCode = 200;
        // }

    }
}