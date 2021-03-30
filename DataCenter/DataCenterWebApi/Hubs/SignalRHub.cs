using System;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.SignalR;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class SignalRHub : Hub
    {
        private readonly IMyLog _logFile;
        private readonly string _localIpAddress;

        public SignalRHub(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, -1).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        // used by web client
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public override async Task OnConnectedAsync()
        {
            //            _logFile.AppendLine($"SignalR Hub: User {Context.User.Identity.Name} connected from = {GetRemoteAddress()} assigned id {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            if (e == null)
            {
                //                _logFile.AppendLine($"OnDisconnectedAsync (ClientIp = {GetRemoteAddress()},  ConnectionId = {Context.ConnectionId})");
                await base.OnDisconnectedAsync(new Exception("SignalR disconnected"));
            }
            else
            {
                _logFile.AppendLine($"OnDisconnectedAsync (ClientIp = {GetRemoteAddress()}). Exception {e.Message}");
                if (e.InnerException != null)
                    _logFile.AppendLine($"Inner exception: {e.InnerException.Message}");
                await base.OnDisconnectedAsync(e);
            }
        }

        public async Task CheckServerIn()
        {
            //            _logFile.AppendLine("Returns DC's connection ID");
            await Clients.All.SendCoreAsync("NotifyServer", new object[] { Context.ConnectionId });
        }

        public async Task NotifyAll(string eventType, string dataInJson)
        {
            try
            {
                await Clients.All.SendAsync(eventType, dataInJson);
                if (eventType != "NotifyMonitoringStep")
                {
                    _logFile.AppendLine($"Hub transmitted {eventType} event");
                    if (eventType == "ServerAsksClientToExit")
                        _logFile.AppendLine($"{dataInJson}");
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"SignalR Hub NotifyAll {eventType}. Exception {e.Message}");

            }
        }

        // send only to user with connectionID
        public async Task SendToOne(string connectionId, string eventType, string dataInJson)
        {
            _logFile.AppendLine($"Hub received {eventType} event");
            await Clients.Client(connectionId).SendAsync(eventType, dataInJson);
        }
    }

}
