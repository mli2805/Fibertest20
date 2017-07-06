using System;
using System.ServiceModel;
using System.Windows;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.RtuWpfExample.D4CWcfServiceReference;
using Iit.Fibertest.RtuWpfExample.D4RWcfServiceReference;
using Iit.Fibertest.RtuWpfExample.RtuWcfServiceReference;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {
        private readonly Logger35 _clientLog;
        private RtuWcfServiceClient _rtuWcfServiceClient;
        private D4RWcfServiceClient _d4RWcfServiceClient;
        private D4CWcfServiceClient _d4CWcfServiceClient;
        private string _rtuServiceIp;
        private string _initResultString;

        public string InitResultString
        {
            get { return _initResultString; }
            set
            {
                if (value == _initResultString) return;
                _initResultString = value;
                NotifyOfPropertyChange();
            }
        }

        public string DcServiceIp { get; set; } = @"192.168.96.179";
        public string RtuServiceIp
        {
            get { return _rtuServiceIp; }
            set
            {
                if (value == _rtuServiceIp) return;
                _rtuServiceIp = value;
                NotifyOfPropertyChange();
            }
        }

        public WcfClientViewModel(IniFile iniFile35)
        {
            _clientLog = new Logger35();
            _clientLog.AssignFile(@"Client.log");

            RtuServiceIp = iniFile35.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");
        }

        public void Initialize()
        {
            var d4CWcfServiceClient = CreateD4CWcfServiceClient(DcServiceIp);
            if (d4CWcfServiceClient == null) return;

            try
            {
                d4CWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"DataCenter to client WCF Service");
                return;
            }
            var rtu = new InitializeRtu() {Id = Guid.NewGuid(), RtuIpAddress = RtuServiceIp, DataCenterIpAddress = DcServiceIp};
            d4CWcfServiceClient.InitializeRtuAsync(rtu);
            _clientLog.AppendLine($@"Sent command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            InitResultString = @"Command sent, wait please.";
        }

        public void StartMonitoring()
        {
            _rtuWcfServiceClient = CreateRtuWcfServiceClient(RtuServiceIp);
            if (_rtuWcfServiceClient == null) return;

            try
            {
                _rtuWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"My WCF Service");
                return;
            }
            _rtuWcfServiceClient.StartMonitoringAsync();
        }

        public void StopMonitoring()
        {
            _rtuWcfServiceClient = CreateRtuWcfServiceClient(RtuServiceIp);
            if (_rtuWcfServiceClient == null) return;

            try
            {
                _rtuWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"My WCF Service");
                return;
            }
            _rtuWcfServiceClient.StopMonitoringAsync();
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
                _clientLog.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _clientLog.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"RtuWcfServiceClient creation error!");
                return null;
            }
        }

        private D4RWcfServiceClient CreateD4RWcfServiceClient(string address)
        {
            try
            {
                var rtuWcfServiceClient = new D4RWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11841, @"D4RWcfService"))));
                _clientLog.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _clientLog.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"D4RWcfServiceClient creation error!");
                return null;
            }
        }

        private D4CWcfServiceClient CreateD4CWcfServiceClient(string address)
        {
            try
            {
                var d4CWcfServiceClient = new D4CWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11840, @"D4CWcfService"))));
                _clientLog.AppendLine($@"Wcf client to {address} created");
                return d4CWcfServiceClient;
            }
            catch (Exception e)
            {
                _clientLog.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"D4CWcfServiceClient creation error!");
                return null;
            }
        }

        private NetTcpBinding CreateDefaultNetTcpBinding()
        {
            return new NetTcpBinding
            {
                Security = {Mode = SecurityMode.None},
                ReceiveTimeout = new TimeSpan(0, 15, 0),
                SendTimeout = new TimeSpan(0, 15, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                MaxBufferSize = 4096000 //4M
            };
        }
    }
}
