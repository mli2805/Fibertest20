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
        void Initialize();
        Task NotifyAll(string eventType, string dataInJson);
        Task SendToOne(string connectionId, string eventType, string dataInJson);
        Task<bool> CheckServerIn();

        string ServerConnectionId { get; set; }
    }

    public class FtSignalRClient : IFtSignalRClient, IDisposable
    {
        private readonly IMyLog _logFile;
        private readonly CurrentDatacenterParameters _cdp;
        private HubConnection _connection;
        private bool _isWebApiInstalled;
        private string _webApiUrl;

        public string ServerConnectionId { get; set; }

        public FtSignalRClient(IMyLog logFile, CurrentDatacenterParameters cdp)
        {
            _logFile = logFile;
            _cdp = cdp;
        }

        public void Initialize()
        {
            _isWebApiInstalled = _cdp.WebApiBindingProtocol != "none";
            _webApiUrl = $"{_cdp.WebApiBindingProtocol}://{_cdp.WebApiDomainName}:{(int)TcpPorts.WebApiListenTo}/webApiSignalRHub";

        }

        private void Build()
        {
            _connection = new HubConnectionBuilder()
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

            _connection.Closed += async (error) =>
            {
                //                _logFile.AppendLine("FtSignalRClient connection was closed.");
                await Task.Delay(1);
            };

            _connection.On<string>("NotifyServer", connId =>
            {
                ServerConnectionId = connId;
                //                _logFile.AppendLine($"NotifyServer returned id {connId}");
            });
        }


        // DataCenter notifies WebClients
        public async Task NotifyAll(string eventType, string dataInJson)
        {
            if (!_isWebApiInstalled) return;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (isConnected)
                {
                    var unused = _connection.InvokeAsync("NotifyAll", eventType, dataInJson);
                    if (eventType != "NotifyMonitoringStep" && eventType != "NudgeSignalR") // too many
                        _logFile.AppendLine($"FtSignalRClient NotifyAll: {eventType} sent successfully.");
                }
            }
            catch (Exception ex)
            {
                _logFile.AppendLine($"FtSignalRClient NotifyAll: {eventType} " + ex.Message);
            }
        }

        // use it for ClientsMeasurement
        public async Task SendToOne(string connectionId, string eventType, string dataInJson)
        {
            if (!_isWebApiInstalled) return;
            try
            {
                var isConnected = await IsSignalRConnected(false);
                if (isConnected)
                {
                    var unused = _connection.InvokeAsync("SendToOne", connectionId, eventType, dataInJson);
                    _logFile.AppendLine($"FtSignalRClient: {eventType} sent successfully.");
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
                    var unused = _connection.InvokeAsync("CheckServerIn");
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
            if (_connection == null)
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
                if (isLog) _logFile.AppendLine($"SignalR connection state is {_connection.State}");
                await Task.Delay(500);
            }

            if (_connection.State != HubConnectionState.Connected)
            {
                if (isLog) _logFile.AppendLine($"Start signalR connection to {_webApiUrl}");
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception e)
                {
                    if (isLog) _logFile.AppendLine($"FtSignalRClient Start connection: " + e.Message);
                    _connection = null;
                    return false;
                }
                if (isLog) _logFile.AppendLine($"SignalR connection state is {_connection.State}");
                await Task.Delay(500);
            }

            return true;
        }

        public async void Dispose()
        {
            await _connection.DisposeAsync();
        }
    }
}
