using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using WcfTestBench.MonitoringSettings;
using WcfTestBench.RtuState;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;

namespace WcfTestBench
{
    public class WcfClientViewModel : Screen
    {
        internal static ServiceHost MyServiceHost;
        private readonly IMyLog _clientLog;
        private readonly IniFile _clientIni;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly Guid _clientGuid;

        #region Server messages
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
            if (dto.IsConnectionSuccessfull)
            {
                DisplayString = Resources.SID_RTU_connected_successfully_;
            }
            else
            {
                var ping = dto.IsPingSuccessful ? Resources.SID____Ping_passed__OK : Resources.SID_Ping_does_not_pass_;
                DisplayString = string.Format(Resources.SID_Can_t_connect_RTU___0_, ping);
            }
        }
        #endregion

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

        public List<NetAddress> ServerAddressList { get; set; }
        public DoubleAddress DcServiceAddresses { get; set; }

        public List<RtuStation> RtuList { get; set; }

        private RtuStation _selectedRtu;
        public RtuStation SelectedRtu
        {
            get { return _selectedRtu; }
            set
            {
                if (Equals(value, _selectedRtu)) return;
                _selectedRtu = value;
                NotifyOfPropertyChange();
            }
        }


        public WcfClientViewModel(IniFile iniFile35, IMyLog clientLog)
        {
            _clientLog = clientLog;
            _clientIni = iniFile35;
            _clientGuid = Guid.Parse(_clientIni.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()));
            var clientAddresses = _clientIni.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);

            ServerAddressList = GetServerAddressList();
            DcServiceAddresses = _clientIni.ReadDoubleAddress((int)TcpPorts.ServerListenToClient);

            RtuList = ReadDbTempTxt();
            SelectedRtu = RtuList.First();

            _c2DWcfManager = new C2DWcfManager(DcServiceAddresses, _clientIni, _clientLog, _clientGuid);
            if (!_c2DWcfManager.RegisterClient(new RegisterClientDto() { Addresses = new DoubleAddress() { Main = clientAddresses, HasReserveAddress = false }, UserName = @"Vasya" }))
                MessageBox.Show(@"Cannot register on server!");

            // start 11843 listener
            StartWcfListener();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Test_RTU_communication_functions;
        }

        private void StartWcfListener()
        {
            MyServiceHost?.Close();
            ClientWcfService.ClientLog = _clientLog;
            ClientWcfService.MessageReceived += ProcessServerMessage;
            MyServiceHost = new ServiceHost(typeof(ClientWcfService));
            try
            {
                MyServiceHost.AddServiceEndpoint(typeof(IClientWcfService),
                    WcfFactory.CreateDefaultNetTcpBinding(_clientIni),
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ClientListenTo, @"ClientWcfService"));
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
            var dto = new CheckRtuConnectionDto()
            {
                ClientId = _clientGuid,
                RtuId = SelectedRtu.Id,
                NetAddress = SelectedRtu.PcAddresses.DoubleAddress.Main,
            };
            DisplayString = _c2DWcfManager.CheckRtuConnection(dto) ? @"Check connection started, wait please" : Resources.SID_Error;
        }

        public void Initialize()
        {
            // TODO contemplate thoroughly
            var dto = new InitializeRtuDto() { RtuId = SelectedRtu.Id, RtuAddresses = SelectedRtu.PcAddresses.DoubleAddress };
            dto.ServerAddresses = (DoubleAddress)DcServiceAddresses.Clone();
            dto.ServerAddresses.Main.Port = (int)TcpPorts.ServerListenToRtu;
            dto.ServerAddresses.Reserve.Port = (int)TcpPorts.ServerListenToRtu;

            _c2DWcfManager.InitializeRtu(dto);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }

        private Random gen = new Random();

        public void MonitoringSettings()
        {
            var vm = new MonitoringSettingsViewModel(SelectedRtu, PopulateModel());
            vm.C2DWcfManager = _c2DWcfManager;
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void BaseRefs()
        {
            var openFileDialog = new OpenFileDialog { Filter = @"sor files|*.sor" };
            byte[] buffer = null;
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    buffer = File.ReadAllBytes(openFileDialog.FileName);
                }
                catch (Exception e)
                {
                    _clientLog.AppendLine(e.Message);
                    return;
                }
            }

            var dto = new AssignBaseRefDto()
            {
                RtuId = SelectedRtu.Id,
                OtauPortDto = new OtauPortDto()
                {
                    OtauIp = SelectedRtu.PcAddresses.DoubleAddress.Main.Ip4Address,
                    OtauTcpPort = 23,
                    OpticalPort = 1
                },
                BaseRefs = new List<BaseRefDto>()
                {
                    new BaseRefDto() { BaseRefType = BaseRefType.Precise, SorBytes = buffer},
                    new BaseRefDto() { BaseRefType = BaseRefType.Fast, SorBytes = buffer},
                    new BaseRefDto() { BaseRefType = BaseRefType.Additional, SorBytes = null},
                }
            };

            _c2DWcfManager.AssignBaseRef(dto);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }

        public void RtuState()
        {
            var vm = new RtuStateViewModel(SelectedRtu.Id);
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
            DisplayString = _c2DWcfManager.StartMonitoring(
                new StartMonitoringDto() { RtuId = SelectedRtu.Id }) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        public void StopMonitoring()
        {
            DisplayString = _c2DWcfManager.StopMonitoring(
                new StopMonitoringDto() { RtuId = SelectedRtu.Id }) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        private MonitoringSettingsModel PopulateModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = true,

                Charons = new List<MonitoringCharonModel>()
                {
                    new MonitoringCharonModel(SelectedRtu.PcAddresses.DoubleAddress.Main.Ip4Address, 23) { Title = @"Грушаука 214", IsMainCharon = true, Ports = PopulatePorts(28)},
                    new MonitoringCharonModel(@"172.16.5.57", 11834) { Ports = PopulatePorts(16)},
                    new MonitoringCharonModel(@"172.16.5.57", 11835) { Ports = PopulatePorts(8)}
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
            var wcfRtuConnection = new WcfFactory(SelectedRtu.PcAddresses.DoubleAddress, _clientIni, _clientLog).CreateRtuConnection();
            if (wcfRtuConnection == null)
                return;

            DisplayString = string.Format(Resources.SID_Established_connection_with_RTU__0_, SelectedRtu.PcAddresses.DoubleAddress.Main.Ip4Address);
            var port = new OtauPortDto() { IsPortOnMainCharon = true, OtauTcpPort = 23, OpticalPort = 2 }; // just for test
            if (!wcfRtuConnection.ToggleToPort(port))
            {
                DisplayString = Resources.SID_Cannot_toggle_to_port_;
                return;
            }

            var otdrAddress = SelectedRtu.OtdrIp == @"192.168.88.101"
                ? SelectedRtu.PcAddresses.DoubleAddress.Main.Ip4Address
                : SelectedRtu.OtdrIp;
            StartReflect($@"-fnw -n {otdrAddress} -p 1500");
        }

        private void StartReflect(string args)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var reflectFile = appDir + @"\..\RftsReflect\Reflect.exe";
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

        private List<NetAddress> GetServerAddressList()
        {
            // @"10.1.37.22" by vpn
            var result = new List<NetAddress>
            {
                new NetAddress(@"192.168.96.21", (int) TcpPorts.ServerListenToClient),
                new NetAddress(@"192.168.97.21", (int) TcpPorts.ServerListenToClient),
            };
            var addr = new NetAddress() { IsAddressSetAsIp = false, HostName = @"some.site.by", Port = (int)TcpPorts.ServerListenToClient };
            result.Add(addr);
            return result;
        }

        private List<RtuStation> ReadDbTempTxt()
        {
            var list = new List<RtuStation>();

            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(app);
            if (path == null)
                return null;
            var filename = Path.Combine(path, @"..\Ini\DbTemp.txt");
            if (File.Exists(filename))
            {
                var content = File.ReadAllLines(filename);
                list = content.Select(ParseLine).ToList();
            }
            _clientLog.AppendLine($@"{list.Count} RTU found");
            return list;
        }

        private static RtuStation ParseLine(string line)
        {
            var parts = line.Split(' ');
            var rtuStation = new RtuStation()
            {
                Id = Guid.Parse(parts[0]),
                PcAddresses = new DoubleAddressWithLastConnectionCheck()
                {
                    DoubleAddress = new DoubleAddress()
                    {
                        Main = new NetAddress(parts[1], (int)TcpPorts.RtuListenTo),
                    },
                    LastConnectionOnMain = DateTime.Now,
                },
            };
            if (parts[2] != @"none")
            {
                rtuStation.PcAddresses.DoubleAddress.HasReserveAddress = true;
                rtuStation.PcAddresses.DoubleAddress.Reserve = new NetAddress(parts[2], (int)TcpPorts.RtuListenTo);
                rtuStation.PcAddresses.LastConnectionOnReserve = DateTime.Now;
            }
            rtuStation.OtdrIp = parts[3];
            return rtuStation;
        }
    }
}
