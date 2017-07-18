﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.Utils35;
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
        }

        private void ProcessRtuInitialized(RtuInitializedDto rtu)
        {
            DisplayString = rtu.Serial;
        }

        private void ProcessMonitoringStarted(MonitoringStartedDto ms)
        {
            DisplayString = $@"monitoring started: {ms.IsSuccessful.ToString().ToUpper()}";
        }

        private void ProcessMonitoringStopped(MonitoringStoppedDto ms)
        {
            DisplayString = $@"monitoring stopped: {ms.IsSuccessful.ToString().ToUpper()}";
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

        public void CheckConnection()
        {
            DisplayString = @"Command sent, wait please.";
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            var dto = new CheckRtuConnectionDto() {Ip4Address = RtuServiceIp, IsAddressSetAsIp = true};
            DisplayString = wcfClient.CheckRtuConnection(dto) ? "OK" : "ERROR";
        }

        public void Initialize()
        {
            _clientIni.Write(IniSection.General, IniKey.RtuServiceIp, RtuServiceIp);

            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            var rtu = new InitializeRtuDto() { RtuId = Guid.NewGuid(), RtuIpAddress = RtuServiceIp, DataCenterIpAddress = DcServiceIp };
            wcfClient.InitializeRtuAsync(rtu);
            _clientLog.AppendLine($@"Sent command to initialize RTU {rtu.RtuId} with ip={rtu.RtuIpAddress}");
            DisplayString = @"Command sent, wait please.";
        }

        private Random gen = new Random();
        public void MonitoringSettings()
        {
            var vm = new MonitoringSettingsViewModel("192.168.96.53", PopulateModel());
            vm.WcfManager = new WcfManager(new NetAddress("192.168.96.179", 23));
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void StartMonitoring()
        {
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.StartMonitoringAsync(RtuServiceIp);
            _clientLog.AppendLine($@"Sent command to start monitoring on RTU with ip={RtuServiceIp}");
            DisplayString = @"Command sent, wait please.";
        }

        public void StopMonitoring()
        {
            var wcfClient = ClientToServerWcfFactory.Create(DcServiceIp);
            wcfClient.StopMonitoringAsync(RtuServiceIp);
            _clientLog.AppendLine($@"Sent command to stop monitoring on RTU with ip={RtuServiceIp}");
            DisplayString = @"Command sent, wait please.";
        }

        private MonitoringSettingsModel PopulateModel()
        {
            var model = new MonitoringSettingsModel()
            {
                IsMonitoringOn = true,

                Charons = new List<MonitoringCharonModel>()
                {
                    new MonitoringCharonModel("192.168.96.53", 23) { Title = "Грушаука 214", Ports = PopulatePorts(28)},
                    new MonitoringCharonModel("192.168.96.57", 11834) { Ports = PopulatePorts(16)},
                    new MonitoringCharonModel("192.168.96.57", 11835) { Ports = PopulatePorts(4)}
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
                    TraceTitle = new StringBuilder().Insert(0, "Probability is a quite long word ", gen.Next(4) + 1) + $" p{i}",
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

    }
}
