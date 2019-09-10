﻿using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly ID2RWcfManager _d2RWcfManager;
        private readonly SorFileRepository _sorFileRepository;
        private readonly EventStoreService _eventStoreService;

        private readonly DoubleAddress _serverDoubleAddress;

        public ClientToRtuTransmitter(IniFile iniFile, IMyLog logFile,
            RtuStationsRepository rtuStationsRepository, ID2RWcfManager d2RWcfManager,
            SorFileRepository sorFileRepository, EventStoreService eventStoreService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _rtuStationsRepository = rtuStationsRepository;
            _d2RWcfManager = d2RWcfManager;
            _sorFileRepository = sorFileRepository;
            _eventStoreService = eventStoreService;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }


        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} check RTU {dto.NetAddress.ToStringA()} connection");
            return await _d2RWcfManager.CheckRtuConnection(dto, _iniFile, _logFile);
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} sent initialize RTU {dto.RtuId.First6()} request");

            dto.ServerAddresses = _serverDoubleAddress;
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an idealogical requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            var rtuInitializedDto = await _d2RWcfManager.SetRtuAddresses(dto.RtuAddresses, _iniFile, _logFile).InitializeAsync(dto);
            if (rtuInitializedDto.IsInitialized)
            {
                rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                await _rtuStationsRepository.RegisterRtuAsync(rtuStation);
            }
            var message = rtuInitializedDto.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " + (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);

            return rtuInitializedDto;
        }

    }
}