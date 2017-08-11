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
        private readonly RtuCommandDeliveredDto _rtuCommandDeliveredDto = new RtuCommandDeliveredDto();
        private void WcfServiceForClient_MessageReceived(object msg)
        {
            var result = ProcessClientsMessage(msg);
            switch (result)
            {
                case MessageProcessingResult.TransmittedSuccessfully:
                    break;
                case MessageProcessingResult.FailedToTransmit:
                case MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy:
                    _rtuCommandDeliveredDto.MessageProcessingResult = result;
                    var thread = new Thread(NotifyRtuCommandDelivery);
                    thread.Start();
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

        private void NotifyRtuCommandDelivery()
        {
            new D2CWcfManager(new List<string>() { _rtuCommandDeliveredDto.ClientAddress }, _coreIni, _dcLog)
                .ConfirmRtuCommandDelivered(_rtuCommandDeliveredDto);
        }

        private MessageProcessingResult ProcessClientsMessage(object msg)
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

            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId, IsRtuStarted = false, IsRtuInitialized = false };
            var address = dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName;
            var rtuConnection = new WcfFactory(address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection != null)
            {
                rtuConnection.IsRtuInitialized(dto); // rtu will answer on this request itself;
                return;
            }

            result.IsPingSuccessful = Pinger.Ping(dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName);
            new D2CWcfManager(new List<string>() { dto.ClientAddress }, _coreIni, _dcLog).ConfirmRtuConnectionChecked(result);
        }

        private MessageProcessingResult InitializeRtu(InitializeRtuDto dto)
        {
            _rtuCommandDeliveredDto.ClientAddress = dto.ClientAddress;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            var rtuConnection = new WcfFactory(dto.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;
            rtuConnection.Initialize(dto);

            _dcLog.AppendLine($"Transfered command to initialize RTU {dto.RtuId} with ip={dto.RtuIpAddress}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

        private MessageProcessingResult StartMonitoring(StartMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientAddress = dto.ClientAddress;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            var rtuStation = _rtuStations.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtuStation == null)
                return MessageProcessingResult.UnknownRtu;

            var rtuConnection = new WcfFactory(rtuStation.Addresses.Main.Ip4Address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.StartMonitoring(dto);
            _dcLog.AppendLine($"Transfered command to start monitoring for rtu with ip={rtuStation.Addresses.Main.Ip4Address}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;
        }

        private MessageProcessingResult StopMonitoring(StopMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientAddress = dto.ClientAddress;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            var rtuStation = _rtuStations.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtuStation == null)
                return MessageProcessingResult.UnknownRtu;

            var rtuConnection = new WcfFactory(rtuStation.Addresses.Main.Ip4Address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.StopMonitoring(dto);
            _dcLog.AppendLine($"Transfered command to stop monitoring for rtu with ip={rtuStation.Addresses.Main.Ip4Address}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;
        }

        private MessageProcessingResult AssignBaseRef(AssignBaseRefDto dto)
        {
            _rtuCommandDeliveredDto.ClientAddress = dto.ClientAddress;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            var rtuStation = _rtuStations.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtuStation == null)
                return MessageProcessingResult.UnknownRtu;

            var rtuConnection = new WcfFactory(rtuStation.Addresses.Main.Ip4Address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            var result = rtuConnection.AssignBaseRef(dto);
            _dcLog.AppendLine($"Transfered command to assign base ref to rtu with ip={rtuStation.Addresses.Main.Ip4Address} result {result}");
            return result ? MessageProcessingResult.TransmittedSuccessfully : MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy;

        }

        private MessageProcessingResult ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            _rtuCommandDeliveredDto.ClientAddress = dto.ClientAddress;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            var rtuStation = _rtuStations.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtuStation == null)
                return MessageProcessingResult.UnknownRtu;

            var rtuConnection = new WcfFactory(rtuStation.Addresses.Main.Ip4Address, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return MessageProcessingResult.FailedToTransmit;

            rtuConnection.ApplyMonitoringSettings(dto);
            _dcLog.AppendLine($"Transfered command to apply monitoring settings for rtu with ip={rtuStation.Addresses.Main.Ip4Address}");
            return MessageProcessingResult.TransmittedSuccessfully;
        }

    }
}
