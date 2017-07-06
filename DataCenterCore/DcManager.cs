using System;
using System.ServiceModel;
using DataCenterCore.RtuWcfServiceReference;
using Dto;
using Iit.Fibertest.Utils35;

namespace DataCenterCore
{
    public class DcManager
    {
        private readonly Logger35 _dcLog;
        private readonly IniFile _coreIni;

        public DcManager()
        {
            _coreIni = new IniFile();
            _coreIni.AssignFile("DcCore.ini");
            var cultureString = _coreIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _dcLog = new Logger35();
            _dcLog.AssignFile("DcCore.log", cultureString);
        }

        public bool InitializeRtu(InitializeRtu rtu)
        {
            var rtuWcfServiceClient = CreateRtuWcfServiceClient(rtu.RtuIpAddress);
            if (rtuWcfServiceClient == null)
                return false;

            try
            {
                rtuWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                return false;
            }
            rtuWcfServiceClient.Initialize();

            _dcLog.AppendLine($"Transfered command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            return true;
        }

        private string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

        private RtuWcfServiceClient CreateRtuWcfServiceClient(string address)
        {
            try
            {
                var rtuWcfServiceClient = new RtuWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11842, @"RtuWcfService"))));
                _dcLog.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                return null;
            }
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

    }
}
