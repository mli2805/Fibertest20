﻿using System;
using System.IO;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    public class PortController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly DesktopC2DWcfManager _desktopC2DWcfManager;

        public PortController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _desktopC2DWcfManager = new DesktopC2DWcfManager(iniFile, logFile);
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToDesktopClient);
            _desktopC2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [Authorize]
        [HttpPost("AttachTrace")]
        public async Task<string> AttachTrace()
        {
            try
            {
                string body;
                using (var reader = new StreamReader(Request.Body))
                {
                    body = await reader.ReadToEndAsync();
                }
                _logFile.AppendLine(body);
                var dto = JsonConvert.DeserializeObject<AttachTrace>(body);

                var result = await _desktopC2DWcfManager.SendCommandAsObj(dto);
                return string.IsNullOrEmpty(result) ? null : result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"AttachTrace: {e.Message}");
                return e.Message;
            }
        }

    }
}