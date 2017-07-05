using System;
using System.ServiceModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.RtuWpfExample.D4RWcfServiceReference;
using Iit.Fibertest.RtuWpfExample.RtuWcfServiceReference;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {
        private readonly Logger35 _rtuLogger;
        private RtuWcfServiceClient _rtuWcfServiceClient;
        private D4RWcfServiceClient _d4RWcfServiceClient;
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

        public WcfClientViewModel(IniFile iniFile35, Logger35 rtuLogger)
        {
            _rtuLogger = rtuLogger;
            RtuServiceIp = iniFile35.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");
        }

        public void Initialize()
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
            _rtuWcfServiceClient.InitializeAsync();
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

        public void InitResult()
        {
            _d4RWcfServiceClient = CreateD4RWcfServiceClient(RtuServiceIp);
            if (_d4RWcfServiceClient == null) return;

            try
            {
                _d4RWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"My WCF Service");
                return;
            }
            var sor = new byte[66547];
            var moniResult = new MonitoringResult() {RtuId = Guid.NewGuid(), SorData = sor};
            _d4RWcfServiceClient.SendMonitoringResult(moniResult);
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
                _rtuLogger.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"RtuWcfServiceClient creation error!");
                return null;
            }
        }

        private D4RWcfServiceClient CreateD4RWcfServiceClient(string address)
        {
            try
            {
                var rtuWcfServiceClient = new D4RWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11841, @"D4RWcfService"))));
                _rtuLogger.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"D4RWcfServiceClient creation error!");
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
