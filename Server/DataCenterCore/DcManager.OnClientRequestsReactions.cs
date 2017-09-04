﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
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
            var clientStation = GetClientStation(_rtuCommandDeliveredDto.ClientId);
            if (clientStation == null)
                return;
            new D2CWcfManager(new List<DoubleAddress>() {clientStation.PcAddresses.DoubleAddress}, _coreIni, _dcLog)
                .ConfirmRtuCommandDelivered(_rtuCommandDeliveredDto);
        }

        private MessageProcessingResult ProcessClientsMessage(object msg)
        {
            var dto = msg as RegisterClientDto;
            if (dto != null)
                return RegisterClient(dto);

            var dto2 = msg as UnRegisterClientDto;
            if (dto2 != null)
                return UnRegisterClient(dto2);

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

        private MessageProcessingResult RegisterClient(RegisterClientDto dto)
        {
            _dcLog.AppendLine($"Client {dto.UserName} registered");
            lock (_clientStationsLockObj)
            {
                if (_clientStations.All(c => c.Id != dto.ClientId))
                    _clientStations.Add(new ClientStation()
                    {
                        Id = dto.ClientId,
                        PcAddresses = new DoubleAddressWithLastConnectionCheck()
                        {
                            DoubleAddress = dto.Addresses
                        }
                    });
            }
            return MessageProcessingResult.ProcessedSuccessfully;
        }

        private MessageProcessingResult UnRegisterClient(UnRegisterClientDto dto)
        {
            _dcLog.AppendLine($"Client {dto.ClientId} exited");
            lock (_clientStationsLockObj)
            {
                _clientStations.RemoveAll(c => c.Id == dto.ClientId);
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
            var rtuConnection = new WcfFactory(new DoubleAddress() {Main  = dto.NetAddress}, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection != null)
            {
                rtuConnection.IsRtuInitialized(dto); // rtu will answer on this request itself;
                return;
            }

            // if failed
            result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            var clientStation = GetClientStation(dto.ClientId);
            if (clientStation != null)
                new D2CWcfManager(new List<DoubleAddress>() { clientStation.PcAddresses.DoubleAddress }, _coreIni, _dcLog).ConfirmRtuConnectionChecked(result);
        }

        private ClientStation GetClientStation(Guid clientId)
        {
            lock (_clientStationsLockObj)
            {
                return _clientStations.FirstOrDefault(c => c.Id == clientId);
            }
        }

        private MessageProcessingResult InitializeRtu(InitializeRtuDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            return new D2RWcfManager(dto.RtuAddresses, _coreIni, _dcLog).InitializeRtu(dto);
        }

        private MessageProcessingResult StartMonitoring(StartMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation) 
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _coreIni, _dcLog).StartMonitoring(dto) 
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult StopMonitoring(StopMonitoringDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation) 
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _coreIni, _dcLog).StopMonitoring(dto) 
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult AssignBaseRef(AssignBaseRefDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation) 
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _coreIni, _dcLog).AssignBaseRef(dto) 
                : MessageProcessingResult.UnknownRtu;
        }

        private MessageProcessingResult ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            _rtuCommandDeliveredDto.ClientId = dto.ClientId;
            _rtuCommandDeliveredDto.RtuId = dto.RtuId;

            RtuStation rtuStation;
            return _rtuStations.TryGetValue(dto.RtuId, out rtuStation)
                ? new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _coreIni, _dcLog).ApplyMonitoringSettings(dto)
                : MessageProcessingResult.UnknownRtu;
        }

    }
}
