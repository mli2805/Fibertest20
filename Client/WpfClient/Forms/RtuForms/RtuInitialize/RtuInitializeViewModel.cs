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
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuInitializeViewModel : Screen
    {
        public RtuInitializeModel FullModel { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IMyLog _logFile;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        private bool _isIdle;
        public bool IsIdle
        {
            get => _isIdle;
            set
            {
                if (value == _isIdle) return;
                _isIdle = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsInitializationPermitted));
            }
        }

        private bool _isCloseEnabled;
        public bool IsCloseEnabled
        {
            get { return _isCloseEnabled; }
            set
            {
                if (value == _isCloseEnabled) return;
                _isCloseEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsInitializationPermitted => _currentUser.Role <= Role.Root && IsIdle;

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser,
            IWindowManager windowManager,  IWcfServiceCommonC2D c2RWcfManager,
            IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            IsIdle = true;
            IsCloseEnabled = true;
            _windowManager = windowManager;
            _c2RWcfManager = c2RWcfManager;
            _logFile = logFile;
            _commonStatusBarViewModel = commonStatusBarViewModel;

            FullModel = _globalScope.Resolve<RtuInitializeModel>();
            FullModel.StartFromRtu(rtuLeaf.Id);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async void InitializeRtu()
        {
            if (!FullModel.Validate()) return;

            try
            {
                IsIdle = false;
                IsCloseEnabled = false;

                using (_globalScope.Resolve<IWaitCursor>())
                {

                    if (!await CheckConnectionBeforeInitialization()) return;
                    var rtuMaker = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == (int)TcpPorts.RtuListenTo
                        ? RtuMaker.IIT
                        : RtuMaker.VeEX;
                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;

                    var initializeRtuDto = CreateDto(rtuMaker);
                    var result = await _c2RWcfManager.InitializeRtuAsync(initializeRtuDto);

                    _commonStatusBarViewModel.StatusBarMessage2 = "";

                    ReactRtuInitialized(result);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($@"InitializeRtu : {e.Message}");
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_RTU_initialization_error_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            finally
            {
                IsIdle = true;
                IsCloseEnabled = true;
            }
        }

        private InitializeRtuDto CreateDto(RtuMaker rtuMaker)
        {
            if (FullModel.IsReserveChannelEnabled && FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenTo
                    : (int)TcpPorts.RtuVeexListenTo;

            if (FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenTo
                    : (int)TcpPorts.RtuVeexListenTo;
            return new InitializeRtuDto()
            {
                RtuMaker = rtuMaker, // it depends on which initialization button was pressed

                RtuId = FullModel.OriginalRtu.Id,
                Serial = FullModel.OriginalRtu.Serial, // properties after previous initialization (if it was)
                OwnPortCount = FullModel.OriginalRtu.OwnPortCount,
                Children = FullModel.OriginalRtu.Children,

                RtuAddresses = new DoubleAddress()
                {
                    Main = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = FullModel.IsReserveChannelEnabled,
                    Reserve = FullModel.IsReserveChannelEnabled
                        ? FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                        : null,
                },
                ShouldMonitoringBeStopped =
                    FullModel.OriginalRtu.OwnPortCount ==
                    0, // if it's first initialization for this RTU - monitoring should be stopped - in case it's running somehow
            };
        }

        private async Task<bool> CheckConnectionBeforeInitialization()
        {
            if (!FullModel.MainChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            }
            if (!await FullModel.MainChannelTestViewModel.ExternalTest())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
                return false;
            }

            if (!FullModel.IsReserveChannelEnabled) return true;

            if (!FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            } 
            if (await FullModel.ReserveChannelTestViewModel.ExternalTest()) return true;

            _windowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            return false;
        }

        private void ReactRtuInitialized(RtuInitializedDto dto)
        {
            var message = dto.IsInitialized
                ? $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialized successfully."
                : dto.RtuAddresses != null 
                    ? $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialization failed. " + Environment.NewLine + dto.ErrorMessage
                    : @"RTU initialization failed. " + Environment.NewLine + dto.ErrorMessage;
            _logFile.AppendLine(message);

            if (dto.IsInitialized)
            {
                FullModel.UpdateWithDto(dto);
            }

            ShowInitializationResultMessageBox(dto);
        }

        private void ShowInitializationResultMessageBox(RtuInitializedDto dto)
        {
            MyMessageBoxViewModel vm;
            switch (dto.ReturnCode)
            {
                case ReturnCode.Ok:
                case ReturnCode.RtuInitializedSuccessfully:
                    var msg = dto.Children.Any(c=>!c.Value.IsOk) 
                        ? Resources.SID_RTU_initialized2 
                        : Resources.SID_RTU_initialized_successfully_;
                    vm = new MyMessageBoxViewModel(MessageType.Information, msg);
                    break;
                case ReturnCode.RtuDoesntSupportBop:
                case ReturnCode.RtuTooBigPortNumber:
                    var strs = new List<string>() { dto.ReturnCode.GetLocalizedString(), "", Resources.SID_Detach_BOP_manually, Resources.SID_and_start_initialization_again_ };
                    vm = new MyMessageBoxViewModel(MessageType.Error, strs);
                    break;
                case ReturnCode.OtauInitializationError:
                    var strs2 = new List<string>() { Resources.SID_RTU_initialization_error_, "", dto.ReturnCode.GetLocalizedString() };
                    vm = new MyMessageBoxViewModel(MessageType.Error, strs2, 2);
                    break;
                default:
                    var strs3 = new List<string>() { ReturnCode.RtuInitializationError.GetLocalizedString(), "",  dto.ErrorMessage };
                    vm = new MyMessageBoxViewModel(MessageType.Error, strs3, 2);
                    break;
            }
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
