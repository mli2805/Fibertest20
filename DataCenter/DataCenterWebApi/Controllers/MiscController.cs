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

        public MiscController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            var doubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _webC2DWcfManager.SetServerAddresses(doubleAddress, "webProxy", "localhost");
        }

        [Authorize]
        [HttpGet("About")]
        public async Task<AboutDto> GetAbout()
        {
            var about = await _webC2DWcfManager.GetAboutInJson(User.Identity.Name);
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
        }}
}