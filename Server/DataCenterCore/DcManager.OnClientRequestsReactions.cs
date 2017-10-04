using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        private readonly RtuCommandDeliveredDto _rtuCommandDeliveredDto = new RtuCommandDeliveredDto();

        public void HandleMessage(object msg)
        {
            var result = ProcessClientsMessage(msg);
            switch (result)
            {
                case MessageProcessingResult.TransmittedSuccessfully:
                case MessageProcessingResult.FailedToTransmit:
                case MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy:
                    _rtuCommandDeliveredDto.MessageProcessingResult = result;
                    var thread = new Thread(NotifyRtuCommandDelivery);
                    thread.Start();
                    break;

                case MessageProcessingResult.UnknownMessage:
                    _logFile.AppendLine("Received unknown message.");
                    break;

                case MessageProcessingResult.ProcessedSuccessfully:
                case MessageProcessingResult.FailedToProcess:
                case MessageProcessingResult.NothingToReturn:
                    break;
            }
        }

        private void NotifyRtuCommandDelivery()
        {
            var clientStation = GetClientStation(_rtuCommandDeliveredDto.ClientId);
            if (clientStation == null)
                return;
            new D2CWcfManager(new List<DoubleAddress>() { clientStation.PcAddresses.DoubleAddress }, _iniFile, _logFile)
                .ConfirmRtuCommandDelivered(_rtuCommandDeliveredDto);
        }

        private MessageProcessingResult ProcessClientsMessage(object msg)
        {
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

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var result = new ClientRegisteredDto();

            var clientStation = new ClientStation {
                Id = dto.ClientId,
                PcAddresses = new DoubleAddressWithLastConnectionCheck {
                    DoubleAddress = dto.Addresses } };

            try
            {
                _clientComps.AddOrUpdate(dto.ClientId, clientStation, (id, _) => clientStation);
                result.IsRegistered = true;
                result.ErrorCode = 0;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.IsRegistered = false;
                result.ErrorCode = 2;
            }

            _logFile.AppendLine($"There are {_clientComps.Count} clients");
            return Task.FromResult(result);
        }

        public Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            lock (_clientStationsLockObj)
            {
                _clientStations.RemoveAll(c => c.Id == dto.ClientId);
            }
            return Task.CompletedTask;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var rtuConnection = new WcfFactory(new DoubleAddress() { Main = dto.NetAddress }, _iniFile, _logFile).CreateRtuConnection();
            if (rtuConnection != null)
            {
                result.IsConnectionSuccessfull = true;
            }
            else
            {
                result.IsConnectionSuccessfull = false;
                result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            }
            return Task.FromResult(result);
        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            dto.ServerAddresses = _serverDoubleAddress;
            return new D2RWcfManager(dto.RtuAddresses, _iniFile, _logFile).InitializeRtuAsync(dto);
        }

        private MessageProcessingResult StartMonitoring(StartMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation)
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile).StartMonitoring(dto)
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult StopMonitoring(StopMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation)
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile).StopMonitoring(dto)
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult AssignBaseRef(AssignBaseRefDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation)
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile).AssignBaseRef(dto)
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation)
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile).ApplyMonitoringSettings(dto)
                : MessageProcessingResult.UnknownRtu;
        }

        private ClientStation GetClientStation(Guid clientId)
        {
            lock (_clientStationsLockObj)
            {
                return _clientStations.FirstOrDefault(c => c.Id == clientId);
            }
        }
    }
}
