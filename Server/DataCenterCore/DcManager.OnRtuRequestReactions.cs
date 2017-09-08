using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private bool WcfServiceForRtu_MessageReceived(object msg)
        {
            var dtoC0 = msg as RtuConnectionCheckedDto;
            if (dtoC0 != null)
                return ProcessRtuConnectionChecked(dtoC0);

            var dtoC1 = msg as RtuInitializedDto;
            if (dtoC1 != null)
                return ConfirmRtuInitialized(dtoC1);

            var dtoC2 = msg as MonitoringStartedDto;
            if (dtoC2 != null)
                return ConfirmMonitoringStarted(dtoC2);

            var dtoC3 = msg as MonitoringStoppedDto;
            if (dtoC3 != null)
                return ConfirmMonitoringStopped(dtoC3);

            var dtoC4 = msg as MonitoringSettingsAppliedDto;
            if (dtoC4 != null)
                return ConfirmMonitoringSettingsApplied(dtoC4);

            var dtoC5 = msg as BaseRefAssignedDto;
            if (dtoC5 != null)
                return ConfirmBaseRefAssigned(dtoC5);

            var dtoR0 = msg as KnowRtuCurrentMonitoringStepDto;
            if (dtoR0 != null)
                return ProcessRtuCurrentMonitoringStep(dtoR0);

            var dtoR1 = msg as RtuChecksChannelDto;
            if (dtoR1 != null)
                return ProcessRtuChecksChannel(dtoR1);

            var dtoR2 = msg as MonitoringResultDto;
            if (dtoR2 != null)
                return ProcessMonitoringResult(dtoR2);

            return false;
        }

        #region RTU confirms
        private bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} replied on connection check");
            var clientStation = GetClientStation(dto.ClientId);
            if (clientStation == null)
                return false;
            var addresses = new List<DoubleAddress>() { clientStation.PcAddresses.DoubleAddress };
            new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmRtuConnectionChecked(dto);
            return true;
        }

        private bool ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            var str = dto.IsInitialized ? "OK" : "ERROR";
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} initialization {str}");
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ConfirmRtuInitialized(dto);
        }

        private bool ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} monitoring started: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ConfirmMonitoringStarted(dto);
        }

        private bool ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} monitoring stopped: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ConfirmMonitoringStopped(dto);
        }

        private bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} applied monitoring settings: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ConfirmMonitoringSettingsApplied(dto);
        }

        private bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId.First6()} assigned base ref: {dto.IsSuccessful}");
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ConfirmBaseRefAssigned(dto);
        }
        #endregion

        #region RTU notifies
        private bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            return new D2CWcfManager(GetAllClientsAddresses(), _coreIni, _dcLog).ProcessRtuCurrentMonitoringStep(monitoringStep);
        }

        private bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
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

        private bool ProcessMonitoringResult(MonitoringResultDto result)
        {
            _dcLog.AppendLine(
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
