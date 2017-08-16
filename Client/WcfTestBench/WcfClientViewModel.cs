using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using WcfConnections;
using WcfTestBench.MonitoringSettings;
using WcfTestBench.RtuState;

namespace WcfTestBench
{
    public class WcfClientViewModel : Screen
    {
        internal static ServiceHost MyServiceHost;
        private readonly LogFile _clientLog;
        private readonly IniFile _clientIni;
        private Guid _rtuId;

        private void ProcessServerMessage(object msg)
        {
            var dto = msg as RtuCommandDeliveredDto;
            if (dto != null)
            {
                ProcessRtuCommandDelivered(dto);
                return;
            }

            var dto1 = msg as RtuInitializedDto;
            if (dto1 != null)
            {
                ProcessRtuInitialized(dto1);
                return;
            }
            var dto2 = msg as MonitoringStartedDto;
            if (dto2 != null)
            {
                ProcessMonitoringStarted(dto2);
                return;
            }
            var dto3 = msg as MonitoringStoppedDto;
            if (dto3 != null)
            {
                ProcessMonitoringStopped(dto3);
            }
            var dto4 = msg as RtuConnectionCheckedDto;
            if (dto4 != null)
            {
                ProcessRtuConnectionChecked(dto4);
            }
            var dto5 = msg as BaseRefAssignedDto;
            if (dto5 != null)
            {
                ProcessBaseRefAssigned(dto5);
            }
            var dto6 = msg as MonitoringSettingsAppliedDto;
            if (dto6 != null)
            {
                ProcessMonitoringSettingsApplied(dto6);
            }
        }

        private void ProcessRtuCommandDelivered(RtuCommandDeliveredDto dto)
        {
            if (dto.MessageProcessingResult == MessageProcessingResult.FailedToTransmit)
                DisplayString = string.Format(Resources.SID_Cannot_deliver_command_to_RTU__0_, dto.RtuId);
            if (dto.MessageProcessingResult == MessageProcessingResult.TransmittedSuccessfullyButRtuIsBusy)
                DisplayString = string.Format(Resources.SID_Command_was_delivered_to_RTU__0__but_RTU_ignored_it__RTU_is_busy_, dto.RtuId);
        }

        private void ProcessRtuInitialized(RtuInitializedDto rtu)
        {
            DisplayString = string.Format(Resources.SID_, rtu.Serial);
            _rtuId = rtu.RtuId;
        }

        private void ProcessMonitoringStarted(MonitoringStartedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Monitoring_started___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessMonitoringStopped(MonitoringStoppedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Monitoring_stopped___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessBaseRefAssigned(BaseRefAssignedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Base_ref_assigned_successfully___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessMonitoringSettingsApplied(MonitoringSettingsAppliedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Monitoring_settings_applied_successfully___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            if (dto.IsRtuInitialized)
            {
                DisplayString = Resources.SID_Rtu_initialized;
            }
            else
            {
                if (dto.IsRtuStarted)
                    DisplayString = Resources.SID_Service_alive;
                else
                    DisplayString = dto.IsPingSuccessful ? Resources.SID____Ping_passed__OK : Resources.SID_Ping_does_not_pass_;
            }
        }

        private string _displayString;

        public string DisplayString
        {
            get { return _displayString; }
            set
            {
                if (value == _displayString) return;
                _displayString = value;
                NotifyOfPropertyChange();
            }
        }

        public DoubleAddressWithLastConnectionCheck DcServiceAddresses { get; set; }
        public DoubleAddressWithLastConnectionCheck RtuAddresses { get; set; } = new DoubleAddressWithLastConnectionCheck();
        private string _rtuServiceIp;
        public string RtuServiceIp
        {
            get { return _rtuServiceIp; }
            set
            {
                if (value == _rtuServiceIp) return;
                _rtuServiceIp = value;
                RtuAddresses.Main.Ip4Address = _rtuServiceIp;
                NotifyOfPropertyChange();
            }
        }


        private C2DWcfManager _c2DWcfManager;
        public WcfClientViewModel(IniFile iniFile35, LogFile clientLog)
        {
            _clientLog = clientLog;
            _clientIni = iniFile35;
            //            DcServiceAddresses = _clientIni.Read(IniSection.Server, IniKey.MainAddressIp, @"10.1.37.22");
            DcServiceAddresses = _clientIni.ReadServerAddresses();
            RtuServiceIp = _clientIni.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");
            _rtuId = Guid.Parse(@"f3e0d85f-2cb3-4160-99ca-408cfd18d765");
            var localIp = _clientIni.Read(IniSection.General, IniKey.LocalIp, @"192.168.96.179");

            _c2DWcfManager = new C2DWcfManager(DcServiceAddresses, _clientIni, _clientLog, localIp);

            if (!_c2DWcfManager.RegisterClient(new RegisterClientDto() {ClientName = @"Vasya"}))
                MessageBox.Show(@"Cannot register on server!");

            // start 11843 listener
            StartWcfListener();
        }

        private void StartWcfListener()
        {
            MyServiceHost?.Close();
            ClientWcfService.ClientLog = _clientLog;
            ClientWcfService.MessageReceived += ProcessServerMessage;
            MyServiceHost = new ServiceHost(typeof(ClientWcfService));
            try
            {
                MyServiceHost.Open();
            }
            catch (Exception e)
            {
                _clientLog.AppendLine(e.Message);
                throw;
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            _c2DWcfManager.UnRegisterClient(new UnRegisterClientDto());
            base.CanClose(callback);
        }

        public void CheckConnection()
        {
            DisplayString = Resources.SID_Command_sent__wait_please_;
            var dto = new CheckRtuConnectionDto() {ClientAddress = @"192.168.96.179", RtuId = Guid.NewGuid(), NetAddress = new NetAddress() { Ip4Address = RtuServiceIp, IsAddressSetAsIp = true}};
            DisplayString = _c2DWcfManager.CheckRtuConnection(dto) ? @"Check connection started, wait please" : Resources.SID_Error;
        }

        public void Initialize()
        {
            _clientIni.Write(IniSection.General, IniKey.RtuServiceIp, RtuServiceIp);

            var rtu = new InitializeRtuDto() { RtuId = _rtuId, ServerAddresses = DcServiceAddresses, RtuAddresses = RtuAddresses };
            _c2DWcfManager.InitializeRtu(rtu);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }

        private Random gen = new Random();
        public void MonitoringSettings()
        {
            var vm = new MonitoringSettingsViewModel(_rtuId, PopulateModel());
            vm.C2DWcfManager = _c2DWcfManager;
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void BaseRefs()
        {
            var dto = new AssignBaseRefDto()
            {
                RtuId = _rtuId,
                OtauPortDto = new OtauPortDto()
                {
                    Ip = _rtuServiceIp,
                    TcpPort = 23,
                    OpticalPort = 3
                },
                BaseRefs = new List<BaseRefDto>()
                {
                    new BaseRefDto() { BaseRefType = BaseRefType.Precise, SorBytes = new byte[32567]},
                    new BaseRefDto() { BaseRefType = BaseRefType.Fast, SorBytes = new byte[31418]},
                    new BaseRefDto() { BaseRefType = BaseRefType.Additional, SorBytes = null},
                }
            };

            _c2DWcfManager.AssignBaseRef(dto);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }
       
        public void RtuState()
        {
            var vm = new RtuStateViewModel(_rtuId);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }

        public void TraceState()
        {
            var vm = new TraceStateViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }

        public void StartMonitoring()
        {
            DisplayString = _c2DWcfManager.StartMonitoring(new StartMonitoringDto() {RtuId = _rtuId}) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        public void StopMonitoring()
        {
            DisplayString = _c2DWcfManager.StopMonitoring(new StopMonitoringDto() {RtuId = _rtuId }) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        private MonitoringSettingsModel PopulateModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = true,

                Charons = new List<MonitoringCharonModel>()
                {
                    new MonitoringCharonModel(_rtuServiceIp, 23) { Title = @"Грушаука 214", IsMainCharon = true, Ports = PopulatePorts(28)},
                    new MonitoringCharonModel(@"192.168.96.57", 11834) { Ports = PopulatePorts(16)},
                    new MonitoringCharonModel(@"192.168.96.57", 11835) { Ports = PopulatePorts(8)}
                }
            };
            model.Frequencies.InitializeComboboxes(Frequency.EveryHour, Frequency.EveryHour, Frequency.EveryHour);
            return model;
        }

        private List<MonitoringPortModel> PopulatePorts(int count)
        {

            var result = new List<MonitoringPortModel>();
            for (int i = 1; i <= count; i++)
            {
                var port = new MonitoringPortModel()
                {
                    PortNumber = i,
                    TraceTitle = new StringBuilder().Insert(0, @"Probability is a quite long word ", gen.Next(4) + 1) + $@" p{i}",
                    IsIncluded = gen.Next(100) <= 25,
                };
                if (port.IsIncluded || gen.Next(100) <= 75)
                {
                    port.PreciseBaseSpan = TimeSpan.FromSeconds(gen.Next(100) + 15);
                    port.FastBaseSpan = TimeSpan.FromSeconds(gen.Next(100) + 15);
                    if (gen.Next(100) <= 2)
                        port.AdditionalBaseSpan = TimeSpan.FromSeconds(gen.Next(100));
                }
                result.Add(port);
            }
            return result;
        }

     
        public void MeasReflect()
        {
            // this is only command which needs direct rtu connection
            var wcfRtuConnection = new WcfFactory(new DoubleAddressWithLastConnectionCheck() {Main = new NetAddress(RtuServiceIp, (int)TcpPorts.RtuListenTo) } , _clientIni, _clientLog).CreateRtuConnection();
            if (wcfRtuConnection == null)
                return;

            DisplayString = string.Format(Resources.SID_Established_connection_with_RTU__0_, RtuServiceIp);
            var port = new OtauPortDto() {Ip = RtuServiceIp, TcpPort = 23, OpticalPort = 5}; // just for test
            if (!wcfRtuConnection.ToggleToPort(port))
            {
                DisplayString = Resources.SID_Cannot_toggle_to_port_;
                return;
            }

            StartReflect($@"-fnw -n {RtuServiceIp} -p 1500");
        }

        private void StartReflect(string args)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var reflectFile = appDir + @"\RftsReflect\Reflect.exe";
            var reflectFolder = Path.GetDirectoryName(reflectFile);
            if (!File.Exists(reflectFile) || reflectFolder == null)
            {
                MessageBox.Show(Resources.SID_Cannot_find_Reflect_exe);
                return;
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = reflectFile,
                    WorkingDirectory = reflectFolder,
                    Arguments = args,
                }
            };
            process.Start();
        }

     
    }
}
