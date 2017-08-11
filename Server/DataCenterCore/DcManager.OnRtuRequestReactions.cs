using System.Collections.Generic;
using System.Linq;
using Dto;
using Iit.Fibertest.Utils35;
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

            var dtoR1 = msg as SaveMonitoringResultDto;
            if (dtoR1 != null)
                return ProcessMonitoringResult(dtoR1);

            var dtoR2 = msg as KnowRtuCurrentMonitoringStepDto;
            if (dtoR2 != null)
                return ProcessRtuCurrentMonitoringStep(dtoR2);

            return false;
        }

        private bool ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId} replied on connection check");
            var addresses = new List<DoubleAddressWithLastConnectionCheck>() {new DoubleAddressWithLastConnectionCheck() {Main = new NetAddress(dto.ClientAddress, (int)TcpPorts.ClientListenTo)} };
            new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmRtuConnectionChecked(dto);
            return true;
        }

        private bool ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            var str = dto.IsInitialized ? "OK" : "ERROR";
            _dcLog.AppendLine($"Rtu {dto.RtuId} initialization {str}");

            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmRtuInitialized(dto);
        }

        private bool ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId} monitoring started: {dto.IsSuccessful}");

            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmMonitoringStarted(dto);
        }

        private bool ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId} monitoring stopped: {dto.IsSuccessful}");

            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmMonitoringStopped(dto);
        }

        private bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId} applied monitoring settings: {dto.IsSuccessful}");

            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmMonitoringSettingsApplied(dto);
        }

        private bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            _dcLog.AppendLine($"Rtu {dto.RtuId} assigned base ref: {dto.IsSuccessful}");

            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ConfirmBaseRefAssigned(dto);
        }

      
        private bool ProcessMonitoringResult(SaveMonitoringResultDto result)
        {
            _dcLog.AppendLine($"Monitoring result received. Sor size is {result.SorData.Length}");
            return true;
        }

        private bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            var addresses = new List<DoubleAddressWithLastConnectionCheck>();
            lock (_clientStationsLockObj)
            {
                addresses.AddRange(_clientStations.Select(clientStation => ((ClientStation)clientStation.Clone()).Addresses));
            }
            return new D2CWcfManager(addresses, _coreIni, _dcLog).ProcessRtuCurrentMonitoringStep(monitoringStep);
        }
    }
}
