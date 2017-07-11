using System;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using RtuManagement.D4RWcfServiceReference;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private string _serverIp;
        private void SendInitializationConfirm(RtuInitialized rtu)
        {
            _serviceLog.AppendLine($"Sending initializatioln result to server...");
            var d4RWcfServiceClient = CreateAndOpenD4RWcfServiceClient();
            if (d4RWcfServiceClient == null)
                return;
            d4RWcfServiceClient.ConfirmInitilization(rtu);
            _serviceLog.AppendLine("Initialization result sent successfully");
        }

        private void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            _rtuLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");
            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            var d4RWcfServiceClient = CreateAndOpenD4RWcfServiceClient();
            d4RWcfServiceClient?.SendMonitoringResult(monitoringResult);
        }

       
        private D4RWcfServiceClient CreateAndOpenD4RWcfServiceClient()
        {
            try
            {
                _serverIp = _rtuIni.Read(IniSection.DataCenter, IniKey.ServerIp, "192.168.96.179");
                var d4RWcfServiceClient = new D4RWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(_serverIp, 11841, @"D4RWcfService"))));
//                _serviceLog.AppendLine($@"Wcf client to {serverIp} created");
                d4RWcfServiceClient.Open();
                return d4RWcfServiceClient;
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

