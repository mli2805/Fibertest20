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

            var result = new RtuConnectionCheckedDto()
            { ClientAddress = param.ClientAddress, IsRtuStarted = true, IsRtuInitialized = IsRtuInitialized };
            new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendCurrentState(result);
        }

        public void SendCurrentMonitoringStep(RtuCurrentMonitoringStep currentMonitoringStep, ExtendedPort extendedPort, BaseRefType baseRefType = BaseRefType.None)
        {
            var dto = new KnowRtuCurrentMonitoringStepDto()
            {
                RtuId = _id,
                MonitoringStep = currentMonitoringStep,
                OtauPort = new OtauPortDto()
                {
                    Ip = extendedPort.NetAddress.Ip4Address,
                    TcpPort = extendedPort.NetAddress.Port,
                    OpticalPort = extendedPort.Port
                },
                BaseRefType = baseRefType,
            };
            new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendCurrentMonitoringStep(dto);
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            var dcConnection = new WcfFactory(_serverIp, _serviceIni, _serviceLog).CreateR2DConnection();
            if (dcConnection == null)
                return;

            var monitoringResult = new SaveMonitoringResultDto() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            dcConnection.ProcessMonitoringResult(monitoringResult);
            _serviceLog.AppendLine($"Sent monitoring result {moniResult.BaseRefType} to server...");
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

