using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class WebApiChecker
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IFtSignalRClient _ftSignalRClient;
        private TimeSpan _checkWebApiEvery;

        public WebApiChecker(IniFile iniFile, IMyLog logFile, IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _ftSignalRClient = ftSignalRClient;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void Check()
        {
            var currentCulture = _iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            var interval = _iniFile.Read(IniSection.General, IniKey.CheckWebApiEvery, 0);
            if (interval == 0) return;
            _checkWebApiEvery = TimeSpan.FromSeconds(interval);
            var bindingProtocol = _iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "http");
            _webApiUrl = $"{bindingProtocol}://localhost:{(int)TcpPorts.WebApiListenTo}/misc/checkapi";
            _logFile.AppendLine($"API check will be carried out at {_webApiUrl}");

            while (true)
            {
                Thread.Sleep(_checkWebApiEvery);
                Tick().Wait();
            }
        }

        private async Task<int> Tick()
        {
            var unused = await CheckApi();

            var isSignalrHubAvailable = await _ftSignalRClient.IsSignalRConnected();
            var word = isSignalrHubAvailable ? "success" : "fail";
            _logFile.AppendLine($"CheckSignalR tick: {word}");

            return 0;
        }

        private static readonly HttpClient httpClient = new HttpClient();
        private string _webApiUrl;
        private async Task<bool> CheckApi()
        {
            try
            {
                var responseString = await httpClient.GetStringAsync(_webApiUrl);
                _logFile.AppendLine($"CheckApi tick: {responseString}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"CheckApi tick: {e.Message}");
                return false;
            }
        }
    }
}