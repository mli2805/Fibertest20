using System;
using Dto;
using Dto.Enums;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private string _serverIp;
        private void SendInitializationConfirm(RtuInitializedDto rtu)
        {
            _serviceLog.AppendLine($"Sending initializatioln result to server...");
            var wcfConnection = RtuToServerWcfFactory.Create(_serverIp);
            wcfConnection?.ProcessRtuInitialized(rtu);
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            _serviceLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");
            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            var wcfConnection = RtuToServerWcfFactory.Create(_serverIp);
            wcfConnection?.ProcessMonitoringResult(monitoringResult);
        }

        private void SendMonitoringStarted(bool isSuccessful)
        {
            var result = new MonitoringStartedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            _serviceLog.AppendLine("Sending start monitoring result");
            var wcfConnection = RtuToServerWcfFactory.Create(_serverIp);
            wcfConnection?.ConfirmStartMonitoring(result);
        }

        private void SendMonitoringStopped(bool isSuccessful)
        {
            var result = new MonitoringStoppedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            _serviceLog.AppendLine("Sending stop monitoring result");
            var wcfConnection = RtuToServerWcfFactory.Create(_serverIp);
            wcfConnection?.ConfirmStopMonitoring(result);
        }

        private void SendMonitoringSettingsApplied(bool isSuccessful)
        {
            var result = new MonitoringSettingsAppliedDto() { RtuIpAddress = _mainCharon.NetAddress.Ip4Address, IsSuccessful = isSuccessful };
            _serviceLog.AppendLine("Sending apply monitoring settings result");
            var wcfConnection = RtuToServerWcfFactory.Create(_serverIp);
            wcfConnection?.ConfirmMonitoringSettingsApplied(result);
        }


        // only whether trace is OK or not, without details of breakdown if any
        private PortMeasResult GetPortState(MoniResult moniResult)
        {
            if (!moniResult.IsFailed && !moniResult.IsFiberBreak && !moniResult.IsNoFiber)
                return PortMeasResult.Ok;

            return moniResult.BaseRefType == BaseRefType.Fast
                ? PortMeasResult.BrokenByFast
                : PortMeasResult.BrokenByPrecise;
        }

    }

    
}

