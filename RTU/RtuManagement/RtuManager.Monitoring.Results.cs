using System;
using System.ServiceModel;
using Dto;
using Dto.Enums;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using RtuManagement.WcfForRtuServiceReference;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private string _serverIp;
        private void SendInitializationConfirm(RtuInitialized rtu)
        {
            _serviceLog.AppendLine($"Sending initializatioln result to server...");
            var wcfClient = CreateAndOpenWcfClient();
            wcfClient?.ProcessRtuInitialized(rtu);
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            _rtuLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");
            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            var wcfClient = CreateAndOpenWcfClient();
            wcfClient?.ProcessMonitoringResult(monitoringResult);
        }

        private void SendMonitoringStarted(bool isSuccessful)
        {
            var result = new MonitoringStarted() {RtuId = _id, IsSuccessful = isSuccessful};
            _rtuLog.AppendLine("Sending start monitorint result");
            var wcfClient = CreateAndOpenWcfClient();
            wcfClient?.ConfirmStartMonitoring(result);
        }

        private void SendMonitoringStopped(bool isSuccessful)
        {
            var result = new MonitoringStopped() {RtuId = _id, IsSuccessful = isSuccessful};
            _rtuLog.AppendLine("Sending stop monitorint result");
            var wcfClient = CreateAndOpenWcfClient();
            wcfClient?.ConfirmStopMonitoring(result);
        }

       
        private WcfServiceForRtuClient CreateAndOpenWcfClient()
        {
            try
            {
                _serverIp = _rtuIni.Read(IniSection.DataCenter, IniKey.ServerIp, "192.168.96.179");
                var wcfClient = new WcfServiceForRtuClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(_serverIp, 11841, @"WcfServiceForRtu"))));
//                _serviceLog.AppendLine($@"Wcf client to {serverIp} created");
                wcfClient.Open();
                return wcfClient;
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                return null;
            }
        }
        private string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

        private NetTcpBinding CreateDefaultNetTcpBinding()
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = new TimeSpan(0, 15, 0),
                SendTimeout = new TimeSpan(0, 15, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                MaxBufferSize = 4096000 //4M
            };
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

