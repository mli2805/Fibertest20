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

        public FtSignalRClient(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            var bindingProtocol = iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "http");
            _webApiUrl = $"{bindingProtocol}://localhost:{(int)TcpPorts.WebApiListenTo}/webApiSignalRHub";
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
                _logFile.AppendLine("FtSignalRClient connection was closed.");
                await Task.Delay(1);
            };
        }

        // DataCenter notifies WebClients
        public async Task NotifyAll(string eventType, string dataInJson)
        {
            try
            {
                if (eventType == "ClientMeasurementDone")
                    _logFile.AppendLine($"FtSignalRClient: have {eventType} signal, need to connect to hub");
                var isConnected = await IsSignalRConnected();
                if (isConnected)
                {
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
            if (connection == null)
            {
                _logFile.AppendLine($"Build signalR connection to {_webApiUrl}");
                try
                {
                    Build();
                }
                catch (Exception e)
                {
                    _logFile.AppendLine($"Build signalR connection: " + e.Message);
                    return false;
                }
                _logFile.AppendLine($"SignalR connection state is {connection.State}");
                await Task.Delay(500);
            }

            if (connection.State != HubConnectionState.Connected)
            {
                _logFile.AppendLine($"Start signalR connection to {_webApiUrl}");
                try
                {
                    await connection.StartAsync();
                }
                catch (Exception e)
                {
                    _logFile.AppendLine($"FtSignalRClient Start connection: " + e.Message);
                    connection = null;
                    //                    if (connection != null)
                    //                    {
                    //                        await connection.DisposeAsync();
                    //                    }
                    return false;
                }
                _logFile.AppendLine($"SignalR connection state is {connection.State}");
                await Task.Delay(500);
            }

            return true;
        }
    }
}
