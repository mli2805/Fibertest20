﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;
        private readonly GlobalState _globalState;
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;
        private readonly IFtSignalRClient _ftSignalRClient;


        public WcfServiceForRtu(IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, D2CWcfManager d2CWcfManager, GlobalState globalState,
            MeasurementsForWebNotifier measurementsForWebNotifier, IFtSignalRClient ftSignalRClient)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
            _globalState = globalState;
            _measurementsForWebNotifier = measurementsForWebNotifier;
            _ftSignalRClient = ftSignalRClient;
        }

        public async void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                await _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                //                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + dto.Step);
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                {
                    _ftSignalRClient.NotifyAll("NotifyMonitoringStep", dto.ToCamelCaseJson()).Wait();
                }

                var addresses = _clientsCollection.GetAllDesktopClientsAddresses();
                if (addresses == null)
                    return;

                _d2CWcfManager.SetClientsAddresses(addresses);
                _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + e.Message);
            }
        }

        public void TransmitClientMeasurementResult(ClientMeasurementResultDto result)
        {
            if (_globalState.IsDatacenterInDbOptimizationMode)
                return;

            var word = (result.SorBytes == null || result.SorBytes.Length == 0)
                ? $": {result.ReturnCode}"
                : $", {result.SorBytes.Length} bytes";
            _logFile.AppendLine($"Measurement Client result for {_clientsCollection.Get(result.ConnectionId)}{word}");

            try
            {
                var client = _clientsCollection.Get(result.ConnectionId);
                if (client == null)
                {
                    _logFile.AppendLine($@"TransmitClientMeasurementResult: client {result.ConnectionId} not found");
                    return;
                }

                if (client.IsWebClient)
                {
                    _measurementsForWebNotifier.Push(result);
                    _logFile.AppendLine("TransmitClientMeasurementResult: measurement placed into queue for web clients");
                }
                else
                {
                    _logFile.AppendLine(
                        $@"TransmitClientMeasurementResult: meas will be sent to desktop client {_clientsCollection.Get(result.ConnectionId)}");
                    _d2CWcfManager.SetClientsAddresses(new List<DoubleAddress>()
                    {
                        new DoubleAddress()
                            { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) }
                    });
                    _d2CWcfManager.NotifyMeasurementClientDone(result).Wait();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.TransmitClientMeasurementResult: " + e.Message);
            }
            _logFile.AppendLine("Client measurement ended");
        }
    }
}
