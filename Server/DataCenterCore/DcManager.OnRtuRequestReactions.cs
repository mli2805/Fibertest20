﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        #region RTU confirms
        public bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} replied on connection check");
            var clientStation = GetClientStation(dto.ClientId);
            if (clientStation == null)
                return false;
            var addresses = new List<DoubleAddress>() { clientStation.PcAddresses.DoubleAddress };
            new D2CWcfManager(addresses, _iniFile, _logFile).ConfirmRtuConnectionChecked(dto);
            return true;
        }

        public bool ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            var str = dto.IsInitialized ? "OK" : "ERROR";
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} initialization {str}");

           if (dto.IsInitialized)
                RegisterRtu(dto);

            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ConfirmRtuInitialized(dto);
        }

        public void RegisterRtu(RtuInitializedDto dto)
        {
            var rtuStation = new RtuStation()
            {
                Id = dto.RtuId,
                OtdrIp = dto.OtdrAddress.Ip4Address,
                PcAddresses = new DoubleAddressWithLastConnectionCheck() { DoubleAddress = dto.PcDoubleAddress },
                Version = dto.Version,
            };

            if (_rtuStations.ContainsKey(dto.RtuId))
                _rtuStations[dto.RtuId] = rtuStation;
            else
                _rtuStations.TryAdd(rtuStation.Id, rtuStation);

            // temporary
            WriteDbTempTxt();
        }

        public bool ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} monitoring started: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ConfirmMonitoringStarted(dto);
        }

        public bool ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} monitoring stopped: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ConfirmMonitoringStopped(dto);
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} applied monitoring settings: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ConfirmMonitoringSettingsApplied(dto);
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            _logFile.AppendLine($"Rtu {dto.RtuId.First6()} assigned base ref: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ConfirmBaseRefAssigned(dto);
        }
        #endregion

        #region RTU notifies
        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            return new D2CWcfManager(GetAllClientsAddresses(), _iniFile, _logFile).ProcessRtuCurrentMonitoringStep(monitoringStep);
        }

        public bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
        {
            RtuStation rtuStation;
            if (_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
            {
                if (dto.IsMainChannel)
                    rtuStation.PcAddresses.LastConnectionOnMain = DateTime.Now;
                else
                    rtuStation.PcAddresses.LastConnectionOnReserve = DateTime.Now;
                rtuStation.Version = dto.Version;
            }
            return true;
        }

        public bool ProcessMonitoringResult(MonitoringResultDto result)
        {
            _logFile.AppendLine(
                $"Moniresult from RTU {result.RtuId.First6()}. {result.BaseRefType} on {result.OtauPort.OpticalPort} port. " +
                $"Trace state is {result.TraceState}. Sor size is {result.SorData.Length}. {result.TimeStamp:yyyy-MM-dd hh-mm-ss}");

//            var filename = $@"c:\temp\sor\{result.RtuId.First6()} {result.TimeStamp:yyyy-MM-dd hh-mm-ss}.sor";
//            var fs = File.Create(filename);
//            fs.Write(result.SorData, 0, result.SorData.Length);
//            fs.Close();

            return true;
        }
        #endregion

        private List<DoubleAddress> GetAllClientsAddresses()
        {
            var addresses = new List<DoubleAddress>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).PcAddresses.DoubleAddress));
            }
            return addresses;
        }
    }
}
