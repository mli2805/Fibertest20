using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.SignalR;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class SignalRHub : Hub
    {
        private readonly IMyLog _logFile;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public SignalRHub(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
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
            await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddressForCommonWcfManager, "onSignalRDisconnected", GetRemoteAddress())
                .UnregisterClientAsync(
                    new UnRegisterClientDto()
                    {
                        ClientIp = GetRemoteAddress(),
                        Username = "onSignalRDisconnected",
                        ConnectionId = Context.ConnectionId,
                    });
        }

        public async Task CheckServerIn()
        {
//            _logFile.AppendLine("Returns DC's connection ID");
            await Clients.All.SendCoreAsync("NotifyServer", new object[] {Context.ConnectionId});
        }

        public async Task NotifyAll(string eventType, string dataInJson)
        {
            _logFile.AppendLine($"Hub received {eventType} event");
            await Clients.All.SendAsync(eventType, dataInJson);
        }

//        public async Task AnswerToThisUser(string eventType, string dataInJson)
//        {
//            _logFile.AppendLine($"Hub received {eventType} event");
//            // TODO send only to this user
//            await Clients.All.SendAsync(eventType, dataInJson);
//        }
    }

}
