using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.WcfConnections
{
    public class C2DWcfManager : IWcfServiceForClient
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        public Guid ClientId { private get; set; }
        private WcfFactory _wcfFactory;

        public C2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void SetServerAddresses(DoubleAddress newServerAddress)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
        }

        public async Task<string> SendCommand(string serializedCmd)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return @"Cannot establish datacenter connection.";

            try
            {
                return await wcfConnection.SendCommand(serializedCmd);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return @"Cannot send command to datacenter.";
            }
        }

        public async Task<string[]> GetEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                // await wcfConnection.GetEvents(revision) blocks client !!!!!!!!!!!
                return wcfConnection.GetEvents(revision).Result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new string[0];
            }
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { IsRegistered = false };

            try
            {
                dto.ClientId = ClientId;
                return await wcfConnection.RegisterClientAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new ClientRegisteredDto() { IsRegistered = false };
            }
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                dto.ClientId = ClientId;
                await wcfConnection.UnregisterClientAsync(dto);
                _logFile.AppendLine($@"Unregistered on server");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return TaskEx.FromResult(false);

            try
            {
                dto.ClientId = ClientId;
                var result = wcfConnection.CheckServerConnection(dto);
                _logFile.AppendLine($@"Server connection is {result.ToString().ToUpper()}");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return TaskEx.FromResult(false);
            }
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($@"Check connection with RTU {dto.NetAddress.ToStringA()}");
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };

            try
            {
                dto.ClientId = ClientId;
                return await c2DChannel.CheckRtuConnectionAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };
            }
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 11, ErrorMessage = "Can't establish connection with DataCenter"};

            try
            {
                _logFile.AppendLine($@"Sent command to initialize RTU {dto.RtuId.First6()}");
                dto.ClientId = ClientId;
                return await c2DChannel.InitializeRtuAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 12, ErrorMessage = e.Message};
            }
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent command to start monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientId = ClientId;
                return await wcfConnection.StartMonitoringAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent command to stop monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientId = ClientId;
                return await wcfConnection.StopMonitoringAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent base ref to RTU {dto.RtuId.First6()}");
                dto.ClientId = ClientId;
                return await wcfConnection.AssignBaseRefAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent monitoring settings to RTU {dto.RtuId.First6()}");
                dto.ClientId = ClientId;
                return await wcfConnection.ApplyMonitoringSettingsAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }
    }
}
