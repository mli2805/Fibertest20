using System;
using System.ServiceModel;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using RtuManagement.D4RWcfServiceReference;

namespace RtuManagement
{
    public partial class RtuManager
    {
        public void SendMonitoringResultToDataCenter(MoniResult moniResult)
        {
            _rtuLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");
            var serverIp = _rtuIni.Read(IniSection.General, IniKey.ServerIp, "192.168.96.179");
            var d4RWcfServiceClient = CreateD4RWcfServiceClient(serverIp);
            if (d4RWcfServiceClient == null) return;

            try
            {
                d4RWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine(e.Message);
                return;
            }
            var monitoringResult = new MonitoringResult() { RtuId = Guid.NewGuid(), SorData = moniResult.SorBytes };
            d4RWcfServiceClient.SendMonitoringResult(monitoringResult);
        }
        private D4RWcfServiceClient CreateD4RWcfServiceClient(string address)
        {
            try
            {
                var rtuWcfServiceClient = new D4RWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11841, @"D4RWcfService"))));
                _rtuLog.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine(e.Message);
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

