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
            var wcfClient = RtuToServerWcfFactory.Create(_serverIp);
            wcfClient?.ProcessRtuInitialized(rtu);
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            _serviceLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");
            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            var wcfClient = RtuToServerWcfFactory.Create(_serverIp);
            wcfClient?.ProcessMonitoringResult(monitoringResult);
        }

        private void SendMonitoringStarted(bool isSuccessful)
        {
            var result = new MonitoringStartedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            _serviceLog.AppendLine("Sending start monitorint result");
            var wcfClient = RtuToServerWcfFactory.Create(_serverIp);
            wcfClient?.ConfirmStartMonitoring(result);
        }

        private void SendMonitoringStopped(bool isSuccessful)
        {
            var result = new MonitoringStoppedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            _serviceLog.AppendLine("Sending stop monitorint result");
            var wcfClient = RtuToServerWcfFactory.Create(_serverIp);
            wcfClient?.ConfirmStopMonitoring(result);
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

