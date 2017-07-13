using System;
using System.ServiceModel;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfTestBench
{
    public class WcfClientViewModel : Screen
    {
        internal static ServiceHost MyServiceHost;
        private readonly Logger35 _clientLog;
        private readonly IniFile _clientIni;
        private readonly string _localIp;
        private string _rtuServiceIp;

        private void ProcessServerMessage(object msg)
        {
            var dto = msg as RtuInitialized;
            if (dto != null)
            {
                ProcessRtuInitialized(dto);
                return;
            }
            var dto2 = msg as MonitoringStarted;
            if (dto2 != null)
            {
                ProcessMonitoringStarted(dto2);
                return;
            }
        }

        private void ProcessRtuInitialized(RtuInitialized rtu)
        {
            InitResultString = rtu.Serial;
        }

        private void ProcessMonitoringStarted(MonitoringStarted ms)
        {
            InitResultString = @"monitoring started";
        }

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
            //            DcServiceIp = _clientIni.Read(IniSection.DataCenter, IniKey.ServerIp, @"10.1.37.22");
            DcServiceIp = _clientIni.Read(IniSection.DataCenter, IniKey.ServerIp, @"192.168.96.179");
            RtuServiceIp = _clientIni.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");

            _localIp = _clientIni.Read(IniSection.General, IniKey.LocalIp, @"192.168.96.179");
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.RegisterClientAsync(_localIp);

            // start 11843 listener
            StartWcf();
        }

        private void StartWcf()
        {
            MyServiceHost?.Close();
            ClientWcfService.ClientLog = _clientLog;
            ClientWcfService.MessageReceived += ProcessServerMessage;
            MyServiceHost = new ServiceHost(typeof(ClientWcfService));
            MyServiceHost.Open();
        }

        public override void CanClose(Action<bool> callback)
        {
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.UnRegisterClientAsync(_localIp);

            base.CanClose(callback);
        }
        public void Initialize()
        {
            _clientIni.Write(IniSection.General, IniKey.RtuServiceIp, RtuServiceIp);

            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            var rtu = new InitializeRtu() { Id = Guid.NewGuid(), RtuIpAddress = RtuServiceIp, DataCenterIpAddress = DcServiceIp };
            wcfClient.InitializeRtuAsync(rtu);
            _clientLog.AppendLine($@"Sent command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            InitResultString = @"Command sent, wait please.";
        }

        public void StartMonitoring()
        {
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.StartMonitoringAsync(RtuServiceIp);
            _clientLog.AppendLine($@"Sent command to start monitoring on RTU with ip={RtuServiceIp}");
        }

        public void StopMonitoring()
        {
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.StopMonitoringAsync(RtuServiceIp);
            _clientLog.AppendLine($@"Sent command to stop monitoring on RTU with ip={RtuServiceIp}");
        }
        
    }
}
