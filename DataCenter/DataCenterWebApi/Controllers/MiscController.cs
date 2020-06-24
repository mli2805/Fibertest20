using System.Diagnostics;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class MiscController : ControllerBase
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public MiscController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet("About")]
        public async Task<AboutDto> GetAbout()
        {
            var about = await _webC2DWcfManager
                .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                .GetAboutInJson(User.Identity.Name);
            _logFile.AppendLine(about == null
                ? "Failed to get about view model"
                : $"json contains {about.Length} symbols");
            var dto = (AboutDto)JsonConvert.DeserializeObject(about, JsonSerializerSettings);
            _logFile.AppendLine(dto == null
                ? "Failed to get dto"
                : $"dto contains {dto.Rtus.Count} items");
            if (dto == null) return null;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            dto.WebApiSoftware = fvi.FileVersion;

            return dto;
        }

        [Authorize]
        [HttpGet("Alarms")]
        public async Task<AlarmsDto> GetCurrentAccidents()
        {
            var currentAccidents = await _webC2DWcfManager
                .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                .GetCurrentAccidents(User.Identity.Name);
            _logFile.AppendLine(currentAccidents == null
                ? "Failed to get current accidents"
                : $"json contains {currentAccidents.Length} symbols");
            var dto = (AlarmsDto)JsonConvert.DeserializeObject(currentAccidents, JsonSerializerSettings);
            _logFile.AppendLine(dto == null
                ? "Failed to get dto"
                : $"dto contains {dto.NetworkAlarms.Count} network alarms, {dto.OpticalAlarms.Count} optical alarms and {dto.BopAlarms.Count} bop alarms"
                );
            return dto;
        }

        [Authorize]
        [HttpGet("Get-sor-file/{sorFileId}")]
        public async Task<string> GetSorFile(int sorFileId)
        {
            var result = new SorBytesDto() { ClientIp = GetRemoteAddress() };
            result.SorBytes = await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                .GetSorBytes(sorFileId);

            if (result.SorBytes == null)
            {
                result.ReturnCode = ReturnCode.Error;
                _logFile.AppendLine($"Failed to get sor file {sorFileId}");
            }
            else
            {
                result.ReturnCode = ReturnCode.Ok;
                _logFile.AppendLine($"json contains {result.SorBytes.Length} symbols");
            }

            var json = JsonConvert.SerializeObject(result);
            return json;
        }
    }
}