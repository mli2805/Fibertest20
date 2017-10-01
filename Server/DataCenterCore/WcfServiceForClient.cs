using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading.Tasks;
using Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using WcfConnections;

namespace DataCenterCore
{
    public class MyListener
    {
        private readonly Action<object> _messageReceived;

        public void RaiseMessageReceived(object e) => _messageReceived(e);

        public MyListener(Action<object> messageReceived)
        {
            _messageReceived = messageReceived ?? throw new ArgumentNullException(nameof(messageReceived));
        }
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServiceForClient : IWcfServiceForClient
    {
        // BUG: Initialize this!
        private readonly EventStoreService _service = new EventStoreService();

        public IniFile ServiceIniFile { get; }
        public IMyLog ServiceLog { get; }

        private readonly MyListener _static; 

        public delegate void OnMessageReceived(object e);

        public ConcurrentDictionary<Guid, ClientStation> ClientComps;

        public string SendCommand(string json) => _service.SendCommand(json);
        public string[] GetEvents(int revision) => _service.GetEvents(revision);

        public WcfServiceForClient(ConcurrentDictionary<Guid, ClientStation> clientComps, MyListener @static, IniFile serviceIniFile, IMyLog serviceLog)
        {
            ClientComps = clientComps;
            _static = @static;
            ServiceIniFile = serviceIniFile;
            ServiceLog = serviceLog;
        }

        public Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} makes an experiment");
            var result = new ClientRegisteredDto();


            if (ClientComps.ContainsKey(dto.ClientId))
            {
                ClientStation oldClient;
                ClientComps.TryRemove(dto.ClientId, out oldClient);
            }

            var client = new ClientStation()
            {
                Id = dto.ClientId,
                PcAddresses = new DoubleAddressWithLastConnectionCheck() { DoubleAddress = dto.Addresses }
            };
            result.IsRegistered = ClientComps.TryAdd(dto.ClientId, client);

            ServiceLog.AppendLine($"There are {ClientComps.Count} clients");
            return Task.FromResult(result);
        }

        public void RegisterClient(RegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent register request");
            _static.RaiseMessageReceived(dto);
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent unregister request");
            _static.RaiseMessageReceived(dto);
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            return true;
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent check rtu {dto.RtuId.First6()} request");
            _static.RaiseMessageReceived(dto);
            return true;
        }



        private D2RWcfManager _d2RWcfManager;
        public void InitializeRtuLongTask(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            _d2RWcfManager = new D2RWcfManager(dto.RtuAddresses, ServiceIniFile, ServiceLog);

            _d2RWcfManager.InitializeRtuLongTask(dto, InitializeRtuLongTaskCallback);
        }

        private void InitializeRtuLongTaskCallback(IAsyncResult asyncState)
        {
            try
            {
                if (_d2RWcfManager == null)
                    return;

                var result = _d2RWcfManager.InitializeRtuLongTaskEnd(asyncState);
                ServiceLog.AppendLine($@"{result.Version}");
            }
            catch (Exception e)
            {
                ServiceLog.AppendLine(e.Message);
            }
        }








        public bool InitializeRtu(InitializeRtuDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent initialize rtu {dto.RtuId.First6()} request");
            _static.RaiseMessageReceived(dto);
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent start monitoring on rtu {dto.RtuId.First6()} request");
            _static.RaiseMessageReceived(dto);
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent stop monitoring on rtu {dto.RtuId.First6()} request");
            _static.RaiseMessageReceived(dto);
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent monitoring settings for rtu {dto.RtuId.First6()}");
            _static.RaiseMessageReceived(dto);
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            ServiceLog.AppendLine($"Client {dto.ClientId.First6()} sent base ref for trace on rtu {dto.RtuId.First6()}");
            _static.RaiseMessageReceived(dto);
            return true;
        }
    }
}
