using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class RtuInitializeViewModel : Screen
    {
        public NetAddressTestViewModel MainChannelTestViewModel { get; set; }
        public bool IsReserveChannelEnabled { get; set; }
        public NetAddressTestViewModel ReserveChannelTestViewModel { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IMyLog _logFile;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        private Rtu _originalRtu;

        public Rtu OriginalRtu
        {
            get => _originalRtu;
            set
            {
                if (Equals(value, _originalRtu)) return;
                _originalRtu = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OtdrAddress));
                NotifyOfPropertyChange(nameof(Bops));
            }
        }

        public string OtdrAddress => OriginalRtu.OtdrNetAddress.Ip4Address == @"192.168.88.101" // fake address on screen
            ? OriginalRtu.MainChannel.Ip4Address
            : OriginalRtu.OtdrNetAddress.Ip4Address;

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> Bops { get; set; }

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser, Model readModel, IWindowManager windowManager,
            IWcfServiceForClient c2DWcfManager, IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _logFile = logFile;
            _commonStatusBarViewModel = commonStatusBarViewModel;

            UpdateView(rtuLeaf.Id);
        }

        private void UpdateView(Guid rtuId)
        {
            OriginalRtu = _readModel.Rtus.First(r => r.Id == rtuId);

            MainChannelTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(OriginalRtu.MainChannel, true)));
            MainChannelTestViewModel.PropertyChanged += MainChannelTestViewModel_PropertyChanged;

            ReserveChannelTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(OriginalRtu.ReserveChannel, true)));
            ReserveChannelTestViewModel.PropertyChanged += ReserveChannelTestViewModel_PropertyChanged;

            IsReserveChannelEnabled = OriginalRtu.IsReserveChannelSet;
            Bops = CreateBops();
        }

        private void ReserveChannelTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (ReserveChannelTestViewModel.Result == true)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Information, Resources.SID_RTU_connection_established_successfully_));
                if (ReserveChannelTestViewModel.Result == false)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            }
        }

        private void MainChannelTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (MainChannelTestViewModel.Result == true)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Information, Resources.SID_RTU_connection_established_successfully_));
                if (MainChannelTestViewModel.Result == false)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async void InitializeRtu()
        {
            if (!Validate()) return;

            try
            {
                IsEditEnabled = false;
                RtuInitializedDto result;
                using (_globalScope.Resolve<IWaitCursor>())
                {
                    if (!await CheckConnectionBeforeInitializaion()) return;

                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;
                    result = await _c2DWcfManager.InitializeRtuAsync(CreateDto());
                    _commonStatusBarViewModel.StatusBarMessage2 = "";
                }
                ProcessRtuInitialized(result);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"InitializeRtu : {e.Message}");
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_RTU_initialization_error_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            IsEditEnabled = true;
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(OriginalRtu.Title))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Title_should_be_set_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            if (!CheckAddressUniqueness())
                return false;
            return true;
        }

        private InitializeRtuDto CreateDto()
        {
            return new InitializeRtuDto()
            {
                RtuId = OriginalRtu.Id,
                Serial = OriginalRtu.Serial, // properties after previous initialization (if it was)
                OwnPortCount = OriginalRtu.OwnPortCount,
                Children = OriginalRtu.Children,

                RtuAddresses = new DoubleAddress()
                {
                    Main = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = IsReserveChannelEnabled,
                    Reserve = IsReserveChannelEnabled
                        ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                        : null,
                },
                ShouldMonitoringBeStopped =
                    OriginalRtu.OwnPortCount ==
                    0, // if it's first initialization for this RTU - monitoring should be stopped - in case it's running somehow
            };
        }

        private async Task<bool> CheckConnectionBeforeInitializaion()
        {
            if (!await MainChannelTestViewModel.ExternalTest())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
                return false;
            }

            if (!IsReserveChannelEnabled) return true;

            if (await ReserveChannelTestViewModel.ExternalTest()) return true;

            _windowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            return false;
        }

        private void ProcessRtuInitialized(RtuInitializedDto dto)
        {
            var message = dto.IsInitialized
                ? $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialized successfully."
                : $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialization failed. " + dto.ErrorMessage;
            _logFile.AppendLine(message);

            if (dto.IsInitialized)
            {
                // apply initialization to graph
                _c2DWcfManager.SendCommandsAsObjs(DtoToCommandList(dto));
                UpdateThisViewModel(dto);
            }

            ShowInitializationResultMessageBox(dto);
        }

        private List<object> DtoToCommandList(RtuInitializedDto dto)
        {
            var commandList = new List<object>();

            // Own port count changed
            if (_originalRtu.OwnPortCount > dto.OwnPortCount)
            {
                var traces = _readModel.Traces.Where(t =>
//                    t.RtuId == dto.RtuId && t.Port >= dto.OwnPortCount && t.OtauPort.IsPortOnMainCharon);
                    t.RtuId == dto.RtuId && t.Port >= dto.OwnPortCount && t.OtauPort.Serial == _originalRtu.Serial);
                foreach (var trace in traces)
                {
                    var cmd = new DetachTrace() { TraceId = trace.TraceId };
                    commandList.Add(cmd);
                }
            }

            // BOP state changed
            foreach (var keyValuePair in dto.Children)
            {
                var bop = _readModel.Otaus.First(o => o.NetAddress.Equals(keyValuePair.Value.NetAddress));
                if (bop.IsOk != keyValuePair.Value.IsOk)
                    commandList.Add(new AddBopNetworkEvent()
                    {
                        EventTimestamp = DateTime.Now,
                        RtuId = dto.RtuId,
                        Serial = keyValuePair.Value.Serial == null ? bop.Serial : keyValuePair.Value.Serial,
                        OtauIp = keyValuePair.Value.NetAddress.Ip4Address,
                        TcpPort = keyValuePair.Value.NetAddress.Port,
                        IsOk = keyValuePair.Value.IsOk,
                    });
            }

            commandList.Add(GetInitializeRtuCommand(dto));
            return commandList;
        }

        private void ShowInitializationResultMessageBox(RtuInitializedDto dto)
        {
            MyMessageBoxViewModel vm;
            switch (dto.ReturnCode)
            {
                case ReturnCode.Ok:
                    vm = new MyMessageBoxViewModel(MessageType.Information,
                        Resources.SID_RTU_initialized_successfully_);
                    break;
                case ReturnCode.RtuDoesntSupportBop:
                case ReturnCode.RtuTooBigPortNumber:
                    var strs = new List<string>() { dto.ReturnCode.GetLocalizedString(), "", Resources.SID_Detach_BOP_manually, Resources.SID_and_start_initialization_again_ };
                    vm = new MyMessageBoxViewModel(MessageType.Error, strs);
                    break;
                default:
                    vm = new MyMessageBoxViewModel(MessageType.Error,
                        ReturnCode.RtuInitializationError.GetLocalizedString());
                    break;
            }
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private void UpdateThisViewModel(RtuInitializedDto dto)
        {
            OriginalRtu = new Rtu
            {
                Id = dto.RtuId,
                Title = OriginalRtu.Title,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                OtdrNetAddress = (NetAddress)dto.OtdrAddress.Clone(),
                Serial = dto.Serial,
                OwnPortCount = dto.OwnPortCount,
                FullPortCount = dto.FullPortCount,
                Version = dto.Version,
                Children = dto.Children,
                Comment = OriginalRtu.Comment,
            };
            Bops = CreateBops();
        }

        private List<string> CreateBops()
        {
            var bops = new List<string>();
            foreach (var pair in OriginalRtu.Children)
            {
                bops.Add(string.Format(Resources.SID____on_port__0___optical_switch__1___, pair.Key, pair.Value.NetAddress.ToStringA()));
                bops.Add(string.Format(Resources.SID_______________________serial__0____1__ports, pair.Value.Serial, pair.Value.OwnPortCount));
            }
            return bops;
        }

        private bool CheckAddressUniqueness()
        {
            var list = _readModel.Rtus.Where(r =>
                r.MainChannel.Ip4Address ==
                MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                r.ReserveChannel.Ip4Address ==
                MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                IsReserveChannelEnabled &&
                (r.MainChannel.Ip4Address ==
                 ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                 r.ReserveChannel.Ip4Address ==
                 ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address)).ToList();

            if (list.Count == 0 || list.Count == 1 && list.First().Id == OriginalRtu.Id) 
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_RTU_with_the_same_ip_address_);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        private InitializeRtu GetInitializeRtuCommand(RtuInitializedDto dto)
        {
            var cmd = new InitializeRtu
            {
                Id = dto.RtuId,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = RtuPartState.Ok,
                IsReserveChannelSet = IsReserveChannelEnabled,
                ReserveChannel = IsReserveChannelEnabled
                    ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                    : null,
                ReserveChannelState = IsReserveChannelEnabled ? RtuPartState.Ok : RtuPartState.NotSetYet,
                OtauNetAddress = dto.OtdrAddress,
                OwnPortCount = dto.OwnPortCount,
                FullPortCount = dto.FullPortCount,
                Serial = dto.Serial,
                Version = dto.Version,
                IsMonitoringOn = dto.IsMonitoringOn,
                Children = dto.Children,
                AcceptableMeasParams = dto.AcceptableMeasParams,
            };
            return cmd;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
