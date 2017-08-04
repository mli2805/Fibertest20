using System;
using Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using WcfConnections;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private string _serverIp;

        public void SendCurrentState(object dto)
        {
            var param = dto as CheckRtuConnectionDto;
            if (param == null)
                return;

            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var result = new RtuConnectionCheckedDto()
                { ClientAddress = param.ClientAddress, IsRtuStarted = true, IsRtuInitialized = IsRtuInitialized };
            dcConnection.ProcessRtuConnectionChecked(result);
            _serviceLog.AppendLine("Sent connection check result to server...");
        }

        private void SendInitializationConfirm(RtuInitializedDto rtu)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            dcConnection.ProcessRtuInitialized(rtu);
            _serviceLog.AppendLine("Sent initializatioln result to server...");
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            dcConnection.ProcessMonitoringResult(monitoringResult);
            _serviceLog.AppendLine($"Sent monitoring result {moniResult.BaseRefType} to server...");
        }

        private void SendMonitoringStarted(bool isSuccessful)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var result = new MonitoringStartedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            dcConnection.ConfirmStartMonitoring(result);
            _serviceLog.AppendLine("Sent start monitoring result");
        }

        private void SendMonitoringStopped(bool isSuccessful)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var result = new MonitoringStoppedDto() {RtuId = _id, IsSuccessful = isSuccessful};
            dcConnection.ConfirmStopMonitoring(result);
            _serviceLog.AppendLine("Sending stop monitoring result");
        }

        private void SendMonitoringSettingsApplied(bool isSuccessful)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var result = new MonitoringSettingsAppliedDto() { RtuIpAddress = _mainCharon.NetAddress.Ip4Address, IsSuccessful = isSuccessful };
            dcConnection.ConfirmMonitoringSettingsApplied(result);
            _serviceLog.AppendLine("Sending apply monitoring settings result");
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

