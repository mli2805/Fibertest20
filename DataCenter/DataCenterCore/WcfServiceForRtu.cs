﻿using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

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
        private readonly FtSignalRClient _ftSignalRClient;

        public WcfServiceForRtu(IniFile iniFile, IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, D2CWcfManager d2CWcfManager, GlobalState globalState,
            FtSignalRClient ftSignalRClient)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
            _globalState = globalState;
            _ftSignalRClient = ftSignalRClient;

            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"RTU listener: works in thread {tid}");
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + dto.Step);
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                {
                    var json = JsonConvert.SerializeObject(dto);
                    _ftSignalRClient.NotifyAll("NotifyMonitoringStep", json).Wait();
                }

                var addresses = _clientsCollection.GetDesktopClientsAddresses();
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

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }

        public void TransmitClientMeasurementResult(SorBytesDto result)
        {
            _logFile.AppendLine($"Measurement Client result received ({result.SorBytes?.Length} bytes, for clientIp {result.ClientIp})");

            if (_globalState.IsDatacenterInDbOptimizationMode)
                return;

            if (result.SorBytes == null || result.SorBytes.Length == 0) return;
            try
            {
                var clientStation = _clientsCollection.GetClientStation(result.ClientIp);
                if (clientStation == null) return;
                if (clientStation.IsWebClient)
                {
                    var json = JsonConvert.SerializeObject(result);
                    _ftSignalRClient.NotifyAll("ClientMeasurementDone", json).Wait();
                }
                else
                {
                    var addresses = _clientsCollection.GetDesktopClientsAddresses(result.ClientIp);
                    if (addresses == null)
                        return;
                    _d2CWcfManager.SetClientsAddresses(addresses);
                    _d2CWcfManager.NotifyMeasurementClientDone(result).Wait();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.TransmitClientMeasurementResult: " + e.Message);
            }
        }
    }
}
