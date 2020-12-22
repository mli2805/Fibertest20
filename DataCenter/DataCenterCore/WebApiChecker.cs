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
        private readonly CurrentDatacenterParameters _cdp;
        private readonly IFtSignalRClient _ftSignalRClient;
        private TimeSpan _checkWebApiEvery;

        public WebApiChecker(IniFile iniFile, IMyLog logFile, 
            CurrentDatacenterParameters cdp, IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _cdp = cdp;
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
            Thread.Sleep(10000);

            var interval = _iniFile.Read(IniSection.General, IniKey.CheckWebApiEvery, 300);
            if (interval == 0) return;
            _checkWebApiEvery = TimeSpan.FromSeconds(interval);
            _webApiUrl = $"{_cdp.WebApiBinding}://localhost:{(int)TcpPorts.WebApiListenTo}/misc/checkapi";
            _logFile.AppendLine($"Web API check will be carried out at {_webApiUrl}");

            while (true)
            {
                Tick().Wait();
                Thread.Sleep(_checkWebApiEvery);
            }
        }

        private bool? _isWebApiAvailable;
        private bool? _isSignalrHubAvailable;
        private async Task<int> Tick()
        {
            _isWebApiAvailable = await CheckApi();
            _isSignalrHubAvailable = await CheckSignalR();
            return 0;
        }

        private async Task<bool> CheckSignalR()
        {
//            var res = await _ftSignalRClient.IsSignalRConnected(false);
            var res = await _ftSignalRClient.CheckServerIn();

//         if (res != _isSignalrHubAvailable)
            {
                _isSignalrHubAvailable = res;
                var word = res ? "success" : "fail";
                _logFile.AppendLine($"CheckSignalR result: {word}");
            }
            return res;
        }

        private static readonly HttpClientHandler httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
        private static readonly HttpClient httpClient = new HttpClient(httpClientHandler);
        private string _webApiUrl;
        private async Task<bool> CheckApi()
        {
            try
            {
                var responseString = await httpClient.GetStringAsync(_webApiUrl);
                if (_isWebApiAvailable != true)
                    _logFile.AppendLine($"CheckApi response: {responseString}");
                return true;
            }
            catch (Exception e)
            {
                if (_isWebApiAvailable != false)
                    _logFile.AppendLine($"CheckApi failed. Exception: {e.Message}");
                return false;
            }
        }
    }
}