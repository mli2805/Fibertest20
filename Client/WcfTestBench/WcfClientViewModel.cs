﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Dto.Enums;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using WcfIntermediary;
using WcfTestBench.MonitoringSettings;

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
            var dto = msg as RtuInitializedDto;
            if (dto != null)
            {
                ProcessRtuInitialized(dto);
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
            
        }

        private void ProcessRtuInitialized(RtuInitializedDto rtu)
        {
            DisplayString = rtu.Serial;
        }

        private void ProcessMonitoringStarted(MonitoringStartedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Monitoring_started___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessMonitoringStopped(MonitoringStoppedDto ms)
        {
            DisplayString = string.Format(Resources.SID_Monitoring_stopped___0_, ms.IsSuccessful.ToString().ToUpper());
        }

        private void ProcessRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            DisplayString = string.Format(Resources.SID_Rtu_connection_checked_Alive, dto.RtuId, dto.IsRtuManagerAlive);
            if (!dto.IsRtuManagerAlive)
                DisplayString += string.Format(Resources.SID_Rtu_connection_checked_Ping, dto.IsPingSuccessful);
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

        private WcfC2DManager _wcfC2DManager;
        public WcfClientViewModel(IniFile iniFile35, Logger35 clientLog)
        {
            _clientLog = clientLog;
            _clientIni = iniFile35;
            //            DcServiceIp = _clientIni.Read(IniSection.DataCenter, IniKey.ServerIp, @"10.1.37.22");
            DcServiceIp = _clientIni.Read(IniSection.DataCenter, IniKey.ServerIp, @"192.168.96.179");
            RtuServiceIp = _clientIni.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");
            _localIp = _clientIni.Read(IniSection.General, IniKey.LocalIp, @"192.168.96.179");

            _wcfC2DManager = new WcfC2DManager(DcServiceIp, _clientIni, _clientLog);

            if (!_wcfC2DManager.RegisterClient(_localIp))
                MessageBox.Show(@"Cannot register on server!");

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
            _wcfC2DManager.UnRegisterClient(_localIp);
            base.CanClose(callback);
        }

        public void CheckConnection()
        {
            DisplayString = Resources.SID_Command_sent__wait_please_;
            var dto = new CheckRtuConnectionDto() {ClientAddress = @"192.168.96.179", RtuId = Guid.NewGuid(), Ip4Address = RtuServiceIp, IsAddressSetAsIp = true};
            DisplayString = _wcfC2DManager.CheckRtuConnection(dto) ? @"Check connection started, wait please" : Resources.SID_Error;
        }

        public void Initialize()
        {
            _clientIni.Write(IniSection.General, IniKey.RtuServiceIp, RtuServiceIp);

            var rtu = new InitializeRtuDto() { RtuId = Guid.NewGuid(), RtuIpAddress = RtuServiceIp, DataCenterIpAddress = DcServiceIp };
            _wcfC2DManager.InitializeRtu(rtu);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }

        private Random gen = new Random();
        public void MonitoringSettings()
        {
            var vm = new MonitoringSettingsViewModel(_rtuServiceIp, PopulateModel());
            vm.WcfC2DManager = _wcfC2DManager;
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void BaseRefs()
        {
            var dto = new AssignBaseRefDto()
            {
                RtuIpAddress = _rtuServiceIp,
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

            _wcfC2DManager.AssignBaseRef(dto);
            DisplayString = Resources.SID_Command_sent__wait_please_;
        }
       
        public void TraceState()
        {
            var vm = new TraceStateViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }

        public void StartMonitoring()
        {
            DisplayString = _wcfC2DManager.StartMonitoring(RtuServiceIp) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        public void StopMonitoring()
        {
            DisplayString = _wcfC2DManager.StopMonitoring(RtuServiceIp) ? Resources.SID_Command_sent__wait_please_ : Resources.SID_Error_;
        }

        private MonitoringSettingsModel PopulateModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = true,

                Charons = new List<MonitoringCharonModel>()
                {
                    new MonitoringCharonModel(_rtuServiceIp, 23) { Title = @"Грушаука 214", Ports = PopulatePorts(28)},
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
            var wcfRtuConnection = new WcfFactory(RtuServiceIp, _clientIni, _clientLog).CreateRtuConnection();
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
