using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.RtuWpfExample
{
    public class OtauViewModel : Screen
    {
        private readonly Logger35 _rtuLogger;

        private string _initializationMessage;
        public string InitializationMessage
        {
            get { return _initializationMessage; }
            set
            {
                if (Equals(value, _initializationMessage)) return;
                _initializationMessage = value;
                NotifyOfPropertyChange(() => InitializationMessage);
            }
        }

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

        public string BopIpAddress { get; set; }
        public int BopTcpPort { get; set; }

        public OtauViewModel(string ipAddress, int tcpPort, Logger35 rtuLogger)
        {
            _rtuLogger = rtuLogger;

            IpAddress = ipAddress;
            OtauTcpPort = tcpPort;

            OtauList = new List<string>();
            BopOtauList = new List<string>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Optical_switch;
        }

        public async Task InitOtau()
        {
            InitializationMessage = Resources.SID_Wait__please___;
            MainCharon = new Charon(new NetAddress() { Ip4Address = IpAddress, Port = OtauTcpPort }, _rtuLogger,
                CharonLogLevel.PublicCommands);
            await RunOtauInitialization();
            InitializationMessage = MainCharon.IsLastCommandSuccessful
                ? Resources.SID_OTAU_initialized_successfully_
                : MainCharon.LastErrorMessage;

            if (!MainCharon.IsLastCommandSuccessful)
                return;

            MainCharonNetAddress = MainCharon.NetAddress.ToStringA();
            var otauList = new List<string> {MainCharon.NetAddress.ToStringA()};
            otauList.AddRange(MainCharon.Children.Values.Select(charon => charon.NetAddress.ToStringA()));
            OtauList = otauList;
            BopOtauList = new List<string>(MainCharon.Children.Values.Select(charon => charon.NetAddress.ToStringA()));

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

        private async Task RunOtauInitialization()
        {
            using (new WaitCursor())
            {
                _rtuLogger.AppendLine(Resources.SID_Otau_initialization_started);
                await Task.Run(() => MainCharon.Initialize());
                if (MainCharon.IsLastCommandSuccessful)
                {
                    _rtuLogger.AppendLine(Resources.SID_Otau_initialized_successfully);
                    IsOtauInitialized = true;
                    NotifyOfPropertyChange(() => CharonInfo);
                    CharonInfo = string.Format(Resources.SID_charon__0__has__1___2__ports, MainCharon.Serial,
                        MainCharon.OwnPortCount, MainCharon.FullPortCount);
                    foreach (var pair in MainCharon.Children)
                    {
                        CharonInfo +=
                            string.Format(Resources.SID_on_port_N_charon_, pair.Key, pair.Value.Serial, pair.Value.NetAddress.ToStringA(), pair.Value.OwnPortCount);
                    }
                }
                else
                {
                    CharonInfo = MainCharon.LastErrorMessage;
                }
            }
        }

        public async Task SetActivePort()
        {
            await Task.Run(()=> MainCharon.SetExtendedActivePort(SelectedNetAddress, NewActivePort));
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

        public void AttachOtau()
        {
            var bopAddress = new NetAddress(BopIpAddress, BopTcpPort);
            MainCharon.AttachOtauToPort(bopAddress, PortForAttachment);
            AttachMessage = MainCharon.IsLastCommandSuccessful 
                ? Resources.SID_Attached_successfully__Press_initialize_main_otau_ 
                : MainCharon.LastErrorMessage;
            _rtuLogger.AppendLine(AttachMessage);
        }

        public void DetachOtau()
        {
            MainCharon.DetachOtauFromPort(DetachPort);
            DetachMessage = MainCharon.IsLastCommandSuccessful
                ? Resources.SID_Detached_successfully__Press_Initialize_main_otau
                : MainCharon.LastErrorMessage;
            _rtuLogger.AppendLine(DetachMessage);
        }
    }
}