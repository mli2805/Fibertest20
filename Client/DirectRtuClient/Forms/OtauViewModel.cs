﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace DirectRtuClient
{
    public class OtauViewModel : Screen
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _rtuLogger;

        private bool _isOtauInitialized;
        public bool IsOtauInitialized
        {
            get { return _isOtauInitialized; }
            set
            {
                if (Equals(value, _isOtauInitialized)) return;
                _isOtauInitialized = value;
                NotifyOfPropertyChange(() => IsOtauInitialized);
            }
        }

        public Charon MainCharon { get; set; }

        private string _mainCharonNetAddress;
        public string MainCharonNetAddress
        {
            get { return _mainCharonNetAddress; }
            set
            {
                if (value == _mainCharonNetAddress) return;
                _mainCharonNetAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public string CharonInfo
        {
            get { return _charonInfo; }
            set
            {
                if (value == _charonInfo) return;
                _charonInfo = value;
                NotifyOfPropertyChange();
            }
        }

        private int _activePort;
        public int ActivePort
        {
            get { return _activePort; }
            set
            {
                if (Equals(value, _activePort)) return;
                _activePort = value;
                NotifyOfPropertyChange(() => ActivePort);
            }
        }

        private int _newActivePort;
        public int NewActivePort
        {
            get { return _newActivePort; }
            set
            {
                if (value == _newActivePort) return;
                _newActivePort = value;
                NotifyOfPropertyChange();
            }
        }

        public int PortForAttachment { get; set; }

        private string _activePortMessage;
        public string ActivePortMessage
        {
            get { return _activePortMessage; }
            set
            {
                if (value == _activePortMessage) return;
                _activePortMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddress ActiveCharonAddress { get; set; }

        public string IpAddress { get; set; }
        public int OtauTcpPort { get; set; }

        private List<string> _otauList;
        public List<string> OtauList
        {
            get { return _otauList; }
            set
            {
                if (Equals(value, _otauList)) return;
                _otauList = value;
                NotifyOfPropertyChange();
            }
        }

        private string _attachMessage;
        public string AttachMessage
        {
            get { return _attachMessage; }
            set
            {
                if (value == _attachMessage) return;
                _attachMessage = value;
                NotifyOfPropertyChange();
            }
        }

        private List<string> _bopOtauList;
        public List<string> BopOtauList
        {
            get { return _bopOtauList; }
            set
            {
                if (Equals(value, _bopOtauList)) return;
                _bopOtauList = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedOtau;
        public string SelectedOtau
        {
            get { return _selectedOtau; }
            set
            {
                if (value == _selectedOtau) return;
                _selectedOtau = value;
                NotifyOfPropertyChange();
            }
        }

        private string _selectedBop;

        public string SelectedBop
        {
            get { return _selectedBop; }
            set
            {
                if (value == null || value == _selectedBop) return;
                _selectedBop = value;
                DetachPort = MainCharon.Children.First(pair => pair.Value.NetAddress.ToStringA() == _selectedBop).Key;
                NotifyOfPropertyChange();
            }
        }

        private int _detachPort;

        public int DetachPort
        {
            get { return _detachPort; }
            set
            {
                if (value == _detachPort) return;
                _detachPort = value;
                NotifyOfPropertyChange();
            }
        }

        private string _detachMessage;
        private string _charonInfo;

        public string DetachMessage
        {
            get { return _detachMessage; }
            set
            {
                if (value == _detachMessage) return;
                _detachMessage = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddress SelectedNetAddress => MainCharon.NetAddress.ToStringA() == SelectedOtau
             ? MainCharon.NetAddress
             : MainCharon.Children.Values.First(a => a.NetAddress.ToStringA() == SelectedOtau).NetAddress;

        public string SelectedSerial => MainCharon.NetAddress.ToStringA() == SelectedOtau
                  ? MainCharon.Serial
                  : MainCharon.Children.Values.First(a => a.NetAddress.ToStringA() == SelectedOtau).Serial;

        private string _bopIpAddress;
        public string BopIpAddress
        {
            get => _bopIpAddress;
            set
            {
                if (value == _bopIpAddress) return;
                _bopIpAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public int BopTcpPort { get; set; } = 11834;

        public OtauViewModel(IniFile iniFile, IMyLog rtuLogger)
        {
            _iniFile35 = iniFile;
            _rtuLogger = rtuLogger;

            IpAddress = _iniFile35.Read(IniSection.RtuManager, IniKey.OtauIp, @"192.168.96.53");
            OtauTcpPort = _iniFile35.Read(IniSection.RtuManager, IniKey.OtauPort, 23);
            BopIpAddress = _iniFile35.Read(IniSection.DirectCharonLibrary, IniKey.BopIpAddress, @"192.168.96.58");

            OtauList = new List<string>();
            BopOtauList = new List<string>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Optical_switch;
        }

        public async Task InitOtau() // button click
        {
            using (new WaitCursor())
            {
                CharonInfo = Resources.SID_Wait__please___;
                MainCharon = new Charon(new NetAddress() { Ip4Address = IpAddress, Port = OtauTcpPort }, true, _iniFile35,
                    _rtuLogger);
                //            await RunOtauInitialization();
                await Task.Run(() => RunOtauInitialization());

                if (!MainCharon.IsLastCommandSuccessful)
                    return;

                MainCharonNetAddress = MainCharon.NetAddress.ToStringA();
                var otauList = new List<string> { MainCharon.NetAddress.ToStringA() };
                otauList.AddRange(MainCharon.Children.Values.Select(charon => charon.NetAddress.ToStringA()));
                OtauList = otauList;
                BopOtauList =
                    new List<string>(MainCharon.Children.Values.Select(charon => charon.NetAddress.ToStringA()));

                await RunOtauGetActivePort();
                if (MainCharon.IsLastCommandSuccessful)
                {
                    SelectedOtau = OtauList.First(a => a == ActiveCharonAddress.ToStringA());
                    NewActivePort = ActivePort;
                    SelectedBop = BopOtauList.FirstOrDefault();
                }
                else
                {
                    SelectedOtau = OtauList.First();
                    NewActivePort = -1;
                }
            }
        }

        public async Task ResetOtau()
        {
            using (new WaitCursor())
            {
                CharonInfo = Resources.SID_Wait__please___;
                _ = await Task.Run(() => MainCharon.ResetOtau());
                CharonInfo = MainCharon.LastAnswer;
            }
        }

        private async Task RunOtauGetActivePort()
        {
            using (new WaitCursor())
            {
                int activePort = -1;
                NetAddress activeCharonAddress = MainCharon.NetAddress;
                await Task.Run(() => MainCharon.GetExtendedActivePort(out activeCharonAddress, out activePort));

                if (MainCharon.IsLastCommandSuccessful)
                {
                    ActiveCharonAddress = activeCharonAddress;
                    ActivePort = activePort;
                    ActivePortMessage = string.Format(Resources.SID_Now_active_is_port__0__on__1_, activePort,
                        activeCharonAddress.ToStringA());
                }
                else
                {
                    ActiveCharonAddress = MainCharon.NetAddress;
                    ActivePort = -1;
                    ActivePortMessage = MainCharon.LastErrorMessage;
                }
                _rtuLogger.AppendLine(ActivePortMessage);
            }
        }

        private void RunOtauInitialization()
        {
            _rtuLogger.AppendLine(Resources.SID_Otau_initialization_started);
            //                await Task.Run(() => MainCharon.InitializeOtau());
            MainCharon.InitializeOtauRecursively();

            if (MainCharon.IsLastCommandSuccessful)
            {
                _rtuLogger.AppendLine(Resources.SID_Otau_initialized_successfully);
                IsOtauInitialized = true;
                NotifyOfPropertyChange(() => CharonInfo);
                CharonInfo = string.Format(Resources.SID_charon__0__has__1___2__ports, MainCharon.Serial,
                    MainCharon.OwnPortCount, MainCharon.FullPortCount);
                foreach (var pair in MainCharon.Children)
                {
                    if (pair.Value.Serial == "" || pair.Value.OwnPortCount == 0)
                        CharonInfo += Resources.SID__Error___;
                    CharonInfo +=
                        string.Format(Resources.SID_on_port_N_charon_, pair.Key, pair.Value.Serial, pair.Value.NetAddress.ToStringA(), pair.Value.OwnPortCount);
                }
            }
            else
            {
                CharonInfo = MainCharon.LastErrorMessage;
            }

        }

        public async Task SetActivePort() // button
        {
            using (new WaitCursor())
            {
                await Task.Run(() => MainCharon.SetExtendedActivePort(SelectedSerial, NewActivePort));
                if (MainCharon.IsLastCommandSuccessful)
                {
                    ActivePortMessage = string.Format(Resources.SID_Now_active_is_port__0__on__1_, NewActivePort,
                        SelectedNetAddress.ToStringA());
                }
                else
                {
                    ActivePortMessage = MainCharon.LastErrorMessage;
                }
                _rtuLogger.AppendLine(ActivePortMessage);
            }
        }

        public void LedOn()
        {
            if (!SelectedNetAddress.Equals(MainCharon.NetAddress))
            {
                var bopCharon = MainCharon.Children.Values.First(a => a.NetAddress.Equals(SelectedNetAddress));
                bopCharon.ShowMessageMeasurementPort();
            }
        }

        public void LedOff()
        {
            if (!SelectedNetAddress.Equals(MainCharon.NetAddress))
            {
                var bopCharon = MainCharon.Children.Values.First(a => a.NetAddress.Equals(SelectedNetAddress));
                bopCharon.ShowOnDisplayMessageReady();
            }
        }

        public void AttachOtau()
        {
            using (new WaitCursor())
            {
                var bopAddress = new NetAddress(BopIpAddress, BopTcpPort);
                // await Task.Run(() => MainCharon.AttachOtauToPort(bopAddress, PortForAttachment));
                MainCharon.AttachOtauToPort(bopAddress, PortForAttachment);
                AttachMessage = MainCharon.IsLastCommandSuccessful
                    ? Resources.SID_Attached_successfully__Press_initialize_main_otau_
                    : MainCharon.LastErrorMessage;
                _rtuLogger.AppendLine(AttachMessage);
            }
        }

        public void DetachOtau()
        {
            using (new WaitCursor())
            {
                MainCharon.DetachOtauFromPort(DetachPort);
                DetachMessage = MainCharon.IsLastCommandSuccessful
                    ? Resources.SID_Detached_successfully__Press_Initialize_main_otau
                    : MainCharon.LastErrorMessage;
                _rtuLogger.AppendLine(DetachMessage);
            }
        }

        public async Task RebootMikrotik()
        {
            var bopIp = SelectedBop.Split(':')[0];
            DetachMessage = @"Mikrotik reboot started...";
            using (new WaitCursor())
            {
                var mikrotik = new MikrotikInBop(_rtuLogger, bopIp, 30);
                if (await Task.Run(() => mikrotik.Connect()))
                    mikrotik.Reboot();

                DetachMessage = MainCharon.IsLastCommandSuccessful
                    ? @"Check Mikrotik in a 40 seconds"
                    : MainCharon.LastErrorMessage;
                _rtuLogger.AppendLine(DetachMessage);
            }
        }

    }
}