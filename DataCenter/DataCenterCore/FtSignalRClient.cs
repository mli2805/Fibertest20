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
        Task<bool> IsSignalRConnected(bool isLog = true);
        Task NotifyAll(string eventType, string dataInJson);
        Task<bool> CheckServerIn();

    }
    public class FtSignalRClient : IFtSignalRClient, IDisposable
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private HubConnection connection;
        private readonly bool _isWebApiInstalled;

        private readonly string _webApiUrl;

        public FtSignalRClient(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            var bindingProtocol = iniFile.Read(IniSection.WebApi, IniKey.BindingProtocol, "http");
            _isWebApiInstalled = bindingProtocol != "none";
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
//                _logFile.AppendLine("FtSignalRClient connection was closed.");
                await Task.Delay(1);
            };

            connection.On<string>("NotifyServer", connId =>
            {
                _clientsCollection.SignalrHubConnectionId = connId;
//                _logFile.AppendLine($"NotifyServer returned id {connId}");
            });
        }

      
        // DataCenter notifies WebClients
        public async Task NotifyAll(string eventType, string dataInJson)
        {
            if (!_isWebApiInstalled) return;
          //  if (!_clientsCollection.HasAnyWebClients()) return;
            try
            {
                if (eventType == "ClientMeasurementDone")
                    _logFile.AppendLine($"FtSignalRClient: have {eventType} signal, need to connect to hub");
                var isConnected = await IsSignalRConnected();
                if (isConnected)
                {
                    var unused = connection.InvokeAsync("NotifyAll", eventType, dataInJson);
                }
            }
            catch (Exception ex)
            {
                _logFile.AppendLine($"FtSignalRClient: {eventType} " + ex.Message);
            }
        }

        public async Task<bool> CheckServerIn()
        {
            if (!_isWebApiInstalled) return true;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (isConnected)
                {
//                    await connection.InvokeAsync("CheckServerIn");
                    var unused = connection.InvokeAsync("CheckServerIn");
                    return true;
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("Exception in FtSignalRClient CheckServerIn: " + e.Message);
            }
            return false;
        }

        public async Task<bool> IsSignalRConnected(bool isLog = true)
        {
            if (!_isWebApiInstalled) return false;
            if (connection == null)
            {
                if (isLog) _logFile.AppendLine($"Build signalR connection to {_webApiUrl}");
                try
                {
                    Build();
                }
                catch (Exception e)
                {
                    if (isLog) _logFile.AppendLine($"Build signalR connection: " + e.Message);
                    return false;
                }
                if (isLog) _logFile.AppendLine($"SignalR connection state is {connection.State}");
                await Task.Delay(500);
            }

            if (connection.State != HubConnectionState.Connected)
            {
                if (isLog) _logFile.AppendLine($"Start signalR connection to {_webApiUrl}");
                try
                {
                    await connection.StartAsync();
                }
                catch (Exception e)
                {
                    if (isLog) _logFile.AppendLine($"FtSignalRClient Start connection: " + e.Message);
                    connection = null;
                    return false;
                }
                if (isLog) _logFile.AppendLine($"SignalR connection state is {connection.State}");
                await Task.Delay(500);
            }

            return true;
        }

        public async void Dispose()
        {
            await connection.DisposeAsync();
        }
    }
}
