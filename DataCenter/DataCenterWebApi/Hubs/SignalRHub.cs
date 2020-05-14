using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Iit.Fibertest.DataCenterWebApi
{
    public class SignalRHub : Hub
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public SignalRHub(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = Context.GetHttpContext().Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        public override async Task OnConnectedAsync()
        {
            _logFile.AppendLine($"OnConnectedAsync ClientIp = {GetRemoteAddress()}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            _logFile.AppendLine($"OnDisconnectedAsync ClientIp = {GetRemoteAddress()}");

            await base.OnDisconnectedAsync(new Exception("SignalR disconnected"));
            await _commonC2DWcfManager
                .SetServerAddresses(_doubleAddressForCommonWcfManager, "", GetRemoteAddress())
                .UnregisterClientAsync(
                    new UnRegisterClientDto()
                    {
                        ClientIp = GetRemoteAddress(),
                        Username = "signalR_disconnected",
                    });
        }


        public async Task Send(string message, string username)
        {
            var processedMessage = await Processor($"{DateTime.Now:HH:mm:ss.FFF} {message}", 1);
            processedMessage = $"{processedMessage} {DateTime.Now:HH:mm:ss.FFF}";
            var signalDto = new SignalDto() { code = 1, message = processedMessage, username = username };
            await Clients.All.SendAsync("RtuInitialized", signalDto);
        }

        [Authorize]
        public async Task InitializeRtu(string rtuId)
        {
            _logFile.AppendLine($"InitializeRtu started: {DateTime.Now:HH:mm:ss.FFF}");
            var result = await LongRtuInitialization(rtuId, GetRemoteAddress());
            _logFile.AppendLine($"InitializeRtu finished: {DateTime.Now:HH:mm:ss.FFF}");
            await Clients.All.SendAsync("RtuInitialized", result);
        }

        private async Task<RtuInitializedWebDto> LongRtuInitialization(string rtuId, string clientIp)
        {
            try
            {
                _logFile.AppendLine($"rtu id = {rtuId}");
                var rtuGuid = Guid.Parse(rtuId);
                var dto = new InitializeRtuDto() { RtuId = rtuGuid, ClientIp = clientIp };
                var rtuInitializedDto = await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, "", GetRemoteAddress())
                    .InitializeRtuAsync(dto);
                if (rtuInitializedDto.ReturnCode == ReturnCode.Ok)
                    rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                _logFile.AppendLine($"LongRtuInitialization: {rtuInitializedDto.ReturnCode}");
                return Map(rtuInitializedDto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"LongRtuInitialization: {e.Message}");
                return new RtuInitializedWebDto();
            }
        }

        private RtuInitializedWebDto Map(RtuInitializedDto dto)
        {
            return new RtuInitializedWebDto()
            {
                RtuId = dto.RtuId,
                ReturnCode = dto.ReturnCode,
                ErrorMessage = dto.ErrorMessage,
                RtuNetworkSettings = new RtuNetworkSettingsDto()
                {
                    MainChannel = dto.RtuAddresses?.Main?.ToStringASpace,
                    IsReserveChannelSet = dto.RtuAddresses?.HasReserveAddress ?? false,
                    ReserveChannel = dto.RtuAddresses?.Reserve?.ToStringASpace,
                    OtdrAddress = dto.OtdrAddress?.ToStringASpace,
                    Mfid = dto.Mfid,
                    Serial = dto.Serial,
                    OwnPortCount = dto.OwnPortCount,
                    FullPortCount = dto.FullPortCount,
                    Version = dto.Version,
                    Version2 = dto.Version2,
                }
            };
        }

        private async Task<string> Processor(string message, int delay)
        {
            await Task.Delay(delay * 1000);
            return message;
        }
    }

    public class SignalDto
    {
        public string username { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
}
