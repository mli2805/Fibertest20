using System;
using System.ServiceModel;
using System.Windows;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.Utils35;
using WcfTestBench.WcfForClientServiceReference;

namespace WcfTestBench
{
    public class WcfClientViewModel : Screen
    {
        private readonly Logger35 _clientLog;
        private readonly IniFile _clientIni;
        private string _localIp;
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

        public string DcServiceIp { get; set; }
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

        public WcfClientViewModel(IniFile iniFile35, Logger35 clientLog)
        {
            _clientLog = clientLog;
            _clientIni = iniFile35;
            DcServiceIp = _clientIni.Read(IniSection.DataCenter, IniKey.ServerIp, @"10.1.37.22");
            RtuServiceIp = _clientIni.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");

            _localIp = _clientIni.Read(IniSection.General, IniKey.LocalIp, @"192.168.96.179");
            var wcfClient = CreateWcfClient(DcServiceIp);
            wcfClient.RegisterClientAsync(_localIp);
        }

        public override void CanClose(Action<bool> callback)
        {
            _clientLog.FreeFile();
            var wcfClient = CreateWcfClient(DcServiceIp);
            wcfClient.UnRegisterClientAsync(_localIp);

            base.CanClose(callback);
        }
        public void Initialize()
        {
            _clientIni.Write(IniSection.General, IniKey.RtuServiceIp, RtuServiceIp);

            var wcfClient = CreateWcfClient(DcServiceIp);
            var rtu = new InitializeRtu() { Id = Guid.NewGuid(), RtuIpAddress = RtuServiceIp, DataCenterIpAddress = DcServiceIp };
            wcfClient.InitializeRtuAsync(rtu);
            _clientLog.AppendLine($@"Sent command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            InitResultString = @"Command sent, wait please.";
        }

        public void StartMonitoring()
        {
            
        }

        public void StopMonitoring()
        {
            
        }

     
        private string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

      private WcfServiceForClientClient CreateWcfClient(string address)
        {
            try
            {
                var wcfClient = new WcfServiceForClientClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11840, @"WcfServiceForClient"))));
                wcfClient.Open();
                return wcfClient;
            }
            catch (Exception e)
            {
                _clientLog.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"WcfServiceForClient creation error!");
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
