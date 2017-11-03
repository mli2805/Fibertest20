﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DbLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var result = await _dbManager.CheckUserPassword(dto);
            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
                return result;


            var clientStation = new ClientStation
            {
                Id = dto.ClientId,
                PcAddresses = new DoubleAddressWithLastConnectionCheck
                {
                    DoubleAddress = dto.Addresses
                }
            };

            try
            {
                _clientStations.AddOrUpdate(dto.ClientId, clientStation, (id, _) => clientStation);
                result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                result.ReturnCode = ReturnCode.ClientRegistrationError;
            }

            _logFile.AppendLine($"There are {_clientStations.Count} clients");
            return result;
        }

        public Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            ClientStation station;
            _clientStations.TryRemove(dto.ClientId, out station);
            _logFile.AppendLine($"There are {_clientStations.Count} clients");
            return Task.CompletedTask;
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var backward = new RtuWcfServiceBackward();
            var wcfFactory = new WcfFactory(new DoubleAddress() { Main = dto.NetAddress }, _iniFile, _logFile);
            var rtuConnection = wcfFactory.CreateDuplexRtuConnection(backward);
            result.IsConnectionSuccessfull = rtuConnection != null;
            if (!result.IsConnectionSuccessfull)
                result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            return result;
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            dto.ServerAddresses = _serverDoubleAddress;
            var rtuInitializedDto = await new D2RWcfManager(dto.RtuAddresses, _iniFile, _logFile).InitializeAsync(dto);
            return rtuInitializedDto;
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            RtuStation rtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
                return false;

            return await new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .StartMonitoringAsync(dto);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            RtuStation rtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
                return false;

            return await new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .StopMonitoringAsync(dto);
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            RtuStation rtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
                return false;

            return await new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .AssignBaseRefAsync(dto);
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            RtuStation rtuStation;
            if (!_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
                return false;

            return await new D2RWcfManager(rtuStation.PcAddresses.DoubleAddress, _iniFile, _logFile)
                .ApplyMonitoringSettingsAsync(dto);
        }




        private ClientStation GetClientStation(Guid clientId)
        {
            return _clientStations.FirstOrDefault(c => c.Key == clientId).Value;
        }
    }
}
