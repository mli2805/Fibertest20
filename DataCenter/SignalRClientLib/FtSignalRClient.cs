using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.SignalR.Client;

namespace Iit.Fibertest.FtSignalRClientLib
{
    public class FtSignalRClient
    {
        private readonly IMyLog _logFile;
        private HubConnection connection;
        private readonly string _webApiUrl;

        public FtSignalRClient(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            var bindingProtocol = iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "http");
            _webApiUrl = $"{bindingProtocol}://localhost:{(int)TcpPorts.WebProxyListenTo}/webApiSignalRHub";
        }

        public void Build()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(_webApiUrl)
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        public async Task Connect()
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                var newMessage = $"{user}: {message}";
                _logFile.AppendLine(newMessage);
            });

            try
            {
                await connection.StartAsync();
                _logFile.AppendLine("FtSignalRClient connection started");
            }
            catch (Exception ex)
            {
                _logFile.AppendLine("FtSignalRClient: Connect: " + ex.Message);
            }
        }

        public async Task NotifyMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                await connection.InvokeAsync("NotifyMonitoringStep", dto);
            }
            catch (Exception ex)
            {
                _logFile.AppendLine("FtSignalRClient: NotifyMonitoringStep: " + ex.Message);
            }
        }

    }
}
