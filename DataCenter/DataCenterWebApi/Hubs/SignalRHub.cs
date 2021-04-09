using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;
using Microsoft.AspNetCore.SignalR;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class SignalRHub : Hub
    {
        private static Dictionary<string, string> Users = new Dictionary<string, string>();

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

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
                userName = "DataCenter?";

            _logFile.AppendLine($"SignalR Hub: User {userName} connected from = {GetRemoteAddress()} assigned id {Context.ConnectionId}");

            Users.TryAdd(Context.ConnectionId, $"{userName} / {GetRemoteAddress()}");
            LogUsers();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            var userName = Context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
                userName = "DataCenter?";

            Users.Remove(Context.ConnectionId);

            if (e == null)
            {
                _logFile.AppendLine($"OnDisconnectedAsync (User {userName} / {GetRemoteAddress()},  ConnectionId = {Context.ConnectionId})");
                await base.OnDisconnectedAsync(new Exception("SignalR disconnected"));
            }
            else
            {
                _logFile.AppendLine($"OnDisconnectedAsync (User {userName} / {GetRemoteAddress()}). Exception {e.Message}");
                if (e.InnerException != null)
                    _logFile.AppendLine($"Inner exception: {e.InnerException.Message}");
                await base.OnDisconnectedAsync(e);
            }

            LogUsers();
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

        // public async Task SendTestToOne(string connectionId, string eventType, string dataInJson)
        // {
        //     try
        //     {
        //         _logFile.AppendLine($"Hub received {dataInJson}");
        //         await Clients.Client(connectionId).SendAsync(eventType, dataInJson);
        //     }
        //     catch (Exception e)
        //     {
        //         _logFile.AppendLine($"SignalR Hub SendTestToOne {eventType}. Exception {e.Message}");
        //     }
        // }

        // send only to user with connectionID
        public async Task SendToOne(string connectionId, string eventType, string dataInJson)
        {
            try
            {
                _logFile.AppendLine($"Hub received {eventType} event for {connectionId}");
                await Clients.Client(connectionId).SendAsync(eventType, dataInJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"SignalR Hub SendToOne {eventType}. Exception {e.Message}");

            }
        }

        private void LogUsers()
        {
            _logFile.EmptyLine();
            _logFile.EmptyLine('-');
            foreach (var pair in Users)
            {
                _logFile.AppendLine($"user: {pair.Value} with signalR ID {pair.Key}");
            }
            _logFile.EmptyLine('-');
            _logFile.EmptyLine();
        }
    }

}
