using System;
using System.Net.Http;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.SignalR.Client;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IFtSignalRClient
    {
        Task<bool> IsSignalRConnected();
        Task NotifyAll(string eventType, string dataInJson);

    }
    public class FtSignalRClient : IFtSignalRClient
    {
        private readonly IMyLog _logFile;
        private HubConnection connection;
        private readonly string _webApiUrl;

        public FtSignalRClient(IMyLog logFile)
        {
            _logFile = logFile;
//            var bindingProtocol = iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "http");
//            _webApiUrl = $"{bindingProtocol}://localhost:{(int)TcpPorts.WebApiListenToDataCenter}/webApiSignalRHub";
            _webApiUrl = $"http://localhost:{(int)TcpPorts.WebApiListenTo}/webApiSignalRHub";
        }

        private void Build()
        {
            connection = new HubConnectionBuilder()
                .WithUrl(_webApiUrl, (opts) =>
                {
                    opts.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // bypass SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) =>
                                {
                                    _logFile.AppendLine($"Negotiation with server returns sslPolicyErrors: {sslPolicyErrors}");
                                    return true;
                                };
                        return message;
                    };
                })
                .Build();

            connection.Closed += async (error) =>
            {
                _logFile.AppendLine("FtSignalRClient connection was closed. Restarting...");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }

        public async Task NotifyAll(string eventType, string dataInJson)
        {
            try
            {
                if (eventType == "ClientMeasurementDone")
                    _logFile.AppendLine($"FtSignalRClient: have {eventType} signal, need to connect to hub");
                var isConnected = await IsSignalRConnected();
                if (isConnected)
                {
//                    _logFile.AppendLine("signalR connection is ready");
                    await connection.InvokeAsync("NotifyAll", eventType, dataInJson);
                }
            }
            catch (Exception ex)
            {
                _logFile.AppendLine($"FtSignalRClient: {eventType} " + ex.Message);
            }
        }

        public async Task<bool> IsSignalRConnected()
        {
            try
            {
                if (connection == null)
                {
                    _logFile.AppendLine($"Build signalR connection to {_webApiUrl}");
                    Build();
                    _logFile.AppendLine($"SignalR connection state is {connection.State}");
                    await Task.Delay(2000);


                    _logFile.AppendLine($"Start signalR connection to {_webApiUrl}");
                    await connection.StartAsync();
                    _logFile.AppendLine($"SignalR connection state is {connection.State}");
                    await Task.Delay(2000);
                }
                else if (connection.State != HubConnectionState.Connected)
                {
                    _logFile.AppendLine($"Start signalR connection to {_webApiUrl}");
                    await connection.StartAsync();
                    _logFile.AppendLine($"SignalR connection state is {connection.State}");
                    await Task.Delay(2000);
                }

                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"FtSignalRClient Start connection: " + e.Message);
                return false;
            }
        }
    }
}
