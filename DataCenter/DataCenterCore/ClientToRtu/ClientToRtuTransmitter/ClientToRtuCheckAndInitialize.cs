﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter : IClientToRtuTransmitter
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly RtuOccupations _rtuOccupations;
        private readonly ID2RWcfManager _d2RWcfManager;
        private readonly IFtSignalRClient _ftSignalRClient;

        private readonly DoubleAddress _serverDoubleAddress;

        public ClientToRtuTransmitter(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, RtuOccupations rtuOccupations, ID2RWcfManager d2RWcfManager,
            IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _rtuOccupations = rtuOccupations;
            _d2RWcfManager = d2RWcfManager;
            _ftSignalRClient = ftSignalRClient;

            _serverDoubleAddress = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }


        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} check RTU {dto.NetAddress.ToStringA()} connection");
            return await _d2RWcfManager.CheckRtuConnection(dto, _iniFile, _logFile);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            dto.ServerAddresses = (DoubleAddress)_serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            string message;
            RtuInitializedDto rtuInitializedDto;
            try
            {
                rtuInitializedDto = await _d2RWcfManager
                    .SetRtuAddresses(dto.RtuAddresses, _iniFile, _logFile)
                    .InitializeAsync(dto);
                if (rtuInitializedDto.IsInitialized)
                {
                    rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                    var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                    await _rtuStationsRepository.RegisterRtuAsync(rtuStation);
                }

                message = rtuInitializedDto.IsInitialized
                    ? "RTU initialized successfully, monitoring mode is " +
                      (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                    : "RTU initialization failed";

            }
            catch (Exception e)
            {
                rtuInitializedDto = new RtuInitializedDto()
                {
                    RtuId = dto.RtuId,
                    IsInitialized = false,
                    ErrorMessage = e.Message
                };
                message = "RTU initialization failed: " + e.Message;
            }
           
            _logFile.AppendLine(message);
            await _ftSignalRClient.NotifyAll("RtuInitialized", rtuInitializedDto.ToCamelCaseJson());

            return rtuInitializedDto;
        }

    }
}