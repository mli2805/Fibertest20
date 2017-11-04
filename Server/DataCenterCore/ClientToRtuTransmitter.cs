﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DatabaseLibrary.DbContexts;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToRtuTransmitter
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IFibertestDbContext _dbContext;
        private readonly RtuRegistrationManager _rtuRegistrationManager;

        private readonly DoubleAddress _serverDoubleAddress;

        public ClientToRtuTransmitter(IniFile iniFile, IMyLog logFile, IFibertestDbContext dbContext, RtuRegistrationManager rtuRegistrationManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _dbContext = dbContext;
            _rtuRegistrationManager = rtuRegistrationManager;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
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
            if (rtuInitializedDto.IsInitialized)
            {
                await _rtuRegistrationManager.RegisterRtuAsync(rtuInitializedDto);
            }
            return rtuInitializedDto;
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            try
            {
                var rtuStation = _dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);

                if (rtuStation != null)
                    return await new D2RWcfManager(rtuStation.GetRtuDoubleAddress(), _iniFile, _logFile)
                        .StartMonitoringAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StartMonitoringAsync:" + e.Message);
                return false;
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            try
            {
                var rtuStation = _dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);

                if (rtuStation != null)
                    return await new D2RWcfManager(rtuStation.GetRtuDoubleAddress(), _iniFile, _logFile)
                        .StopMonitoringAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StopMonitoringAsync:" + e.Message);
                return false;
            }
        }

        public async Task<bool> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            try
            {
                var rtuStation = _dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);

                if (rtuStation != null)
                    return await new D2RWcfManager(rtuStation.GetRtuDoubleAddress(), _iniFile, _logFile)
                        .AssignBaseRefAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync:" + e.Message);
                return false;
            }
        }

        public async Task<bool> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            try
            {
                var rtuStation = _dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);

                if (rtuStation != null)
                    return await new D2RWcfManager(rtuStation.GetRtuDoubleAddress(), _iniFile, _logFile)
                        .ApplyMonitoringSettingsAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ApplyMonitoringSettingsAsync:" + e.Message);
                return false;
            }
        }
    }
}
