using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dto;
using Iit.Fibertest.Utils35;
using WcfConnections;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private void WcfServiceForClient_MessageReceived(object msg)
        {
            var result = ProcessMessage(msg);
            switch (result)
            {
                case MessageProcessingResult.FailedToTransmit:
                case MessageProcessingResult.TransmittedSuccessfully:
                    // TODO send this to client
                    break;

                case MessageProcessingResult.UnknownMessage:
                    _dcLog.AppendLine("Received unknown message.");
                    break;

                case MessageProcessingResult.ProcessedSuccessfully:
                case MessageProcessingResult.FailedToProcess:
                case MessageProcessingResult.NothingToReturn:
                    break;
            }
        }

        private MessageProcessingResult ProcessMessage(object msg)
        {
            var dto = msg as RegisterClientDto;
            if (dto != null)
                return RegisterClient(dto.ClientAddress);

            var dto2 = msg as UnRegisterClientDto;
            if (dto2 != null)
                return UnRegisterClient(dto2.ClientAddress);

            var dto3 = msg as CheckRtuConnectionDto;
            if (dto3 != null)
            {
                CheckRtuConnection(dto3);
                return MessageProcessingResult.NothingToReturn;
            }

            var dto4 = msg as InitializeRtuDto;
            if (dto4 != null)
                return InitializeRtu(dto4);

            var dto5 = msg as StartMonitoringDto;
            if (dto5 != null)
                return StartMonitoring(dto5);

            var dto6 = msg as StopMonitoringDto;
            if (dto6 != null)
                return StopMonitoring(dto6);

            var dto7 = msg as AssignBaseRefDto;
            if (dto7 != null)
                return AssignBaseRef(dto7);

            var dto8 = msg as ApplyMonitoringSettingsDto;
            if (dto8 != null)
                return ApplyMonitoringSettings(dto8);

            return MessageProcessingResult.UnknownMessage;
        }

        private MessageProcessingResult RegisterClient(string address)
        {
            _dcLog.AppendLine($"Client {address} registered");
            lock (_clientStationsLockObj)
            {
                if (_clientStations.All(c => c.Ip != address))
                    _clientStations.Add(new ClientStation() { Ip = address });
            }
            return MessageProcessingResult.ProcessedSuccessfully;
        }

        private MessageProcessingResult UnRegisterClient(string address)
        {
            _dcLog.AppendLine($"Client {address} exited");
            lock (_clientStationsLockObj)
            {
                _clientStations.RemoveAll(c => c.Ip == address);
            }
            return MessageProcessingResult.ProcessedSuccessfully;
        }

        private void CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            Thread thread = new Thread(CheckRtuConnectionThread);
            thread.Start(dto);
        }

        private void CheckRtuConnectionThread(object param)
        {
            var dto = param as CheckRtuConnectionDto;
            if (dto == null)
                return;

            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var address = dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName;
            var rtuConnection = new WcfFactory(address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
            {
                result.IsPingSuccessful = Pinger.Ping(dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName);
            }
            else
            {
                result.IsRtuConnectionSuccessful = true;
                result.IsRtuInitialized = rtuConnection.IsRtuInitialized();
            }
           
            new D2CWcfManager(new List<string>() { dto.ClientAddress }, _coreIni, _dcLog).ConfirmRtuConnectionChecked(result);
        }

        private MessageProcessingResult InitializeRtu(InitializeRtuDto rtu)
        {
            var rtuConnection = new WcfFactory(rtu.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;
            rtuConnection.Initialize(rtu);

            _dcLog.AppendLine($"Transfered command to initialize RTU {rtu.RtuId} with ip={rtu.RtuIpAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        private MessageProcessingResult StartMonitoring(StartMonitoringDto dto)
        {
            var rtuConnection = new WcfFactory(dto.RtuAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.StartMonitoring();
            _dcLog.AppendLine($"Transfered command to start monitoring for rtu with ip={dto.RtuAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        private MessageProcessingResult StopMonitoring(StopMonitoringDto dto)
        {
            var rtuConnection = new WcfFactory(dto.RtuAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.StopMonitoring();
            _dcLog.AppendLine($"Transfered command to stop monitoring for rtu with ip={dto.RtuAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        private MessageProcessingResult AssignBaseRef(AssignBaseRefDto baseRef)
        {
            var rtuConnection = new WcfFactory(baseRef.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.AssignBaseRef(baseRef);
            _dcLog.AppendLine($"Transfered command to assign base ref to rtu with ip={baseRef.RtuIpAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        private MessageProcessingResult ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            var rtuConnection = new WcfFactory(settings.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.ApplyMonitoringSettings(settings);
            _dcLog.AppendLine($"Transfered command to apply monitoring settings for rtu with ip={settings.RtuIpAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

    }
}
