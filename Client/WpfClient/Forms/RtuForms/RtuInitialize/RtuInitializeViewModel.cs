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
        public RtuInitializeModel FullModel { get; set; }

        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly IniFile _iniFile;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
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

        public RtuInitializeViewModel(ILifetimeScope globalScope, CurrentUser currentUser, Model readModel,
            IniFile iniFile, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            IsIdle = true;
            IsCloseEnabled = true;
            _readModel = readModel;
            _iniFile = iniFile;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
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

                    if (!await CheckConnectionBeforeInitializaion()) return;
                    // TODO maybe special type ?
                    var rtuMaker = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == (int)TcpPorts.RtuListenTo
                        ? RtuMaker.IIT
                        : RtuMaker.VeEX;
                    _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;

                    var initializeRtuDto = CreateDto(rtuMaker);
                    var result = await _c2DWcfManager.InitializeRtuAsync(initializeRtuDto);

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

        private async Task<bool> CheckConnectionBeforeInitializaion()
        {
            if (!await FullModel.MainChannelTestViewModel.ExternalTest())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
                return false;
            }

            if (!FullModel.IsReserveChannelEnabled) return true;

            if (await FullModel.ReserveChannelTestViewModel.ExternalTest()) return true;

            _windowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            return false;
        }

        private void ReactRtuInitialized(RtuInitializedDto dto)
        {
            var message = dto.IsInitialized
                ? $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialized successfully."
                : $@"RTU {dto.RtuAddresses.Main.Ip4Address} initialization failed. " + dto.ErrorMessage;
            _logFile.AppendLine(message);

            if (dto.IsInitialized)
            {
                // apply initialization to graph
                _c2DWcfManager.SendCommandsAsObjs(DtoToCommandList(dto));
                var ip4AddressDefault = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address;
                ip4AddressDefault = ip4AddressDefault.Substring(0, ip4AddressDefault.LastIndexOf('.') + 1);
                _iniFile.Write(IniSection.General, IniKey.Ip4Default, ip4AddressDefault);
                FullModel.UpdateWithDto(dto);
            }

            ShowInitializationResultMessageBox(dto);
        }

        private List<object> DtoToCommandList(RtuInitializedDto dto)
        {
            var commandList = new List<object>();

            // Own port count changed
            if (FullModel.OriginalRtu.OwnPortCount > dto.OwnPortCount)
            {
                var traces = _readModel.Traces.Where(t =>
                    t.RtuId == dto.RtuId && t.Port >= dto.OwnPortCount && t.OtauPort.Serial == FullModel.OriginalRtu.Serial);
                foreach (var trace in traces)
                {
                    var cmd = new DetachTrace() { TraceId = trace.TraceId };
                    commandList.Add(cmd);
                }
            }

            // BOP state changed
            if (dto.Children != null)
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
                case ReturnCode.RtuInitializedSuccessfully:
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

        private InitializeRtu GetInitializeRtuCommand(RtuInitializedDto dto)
        {
            var cmd = new InitializeRtu
            {
                Id = dto.RtuId,
                Maker = dto.Maker,
                Mfid = dto.Mfid,
                Mfsn = dto.Mfsn,
                Omid = dto.Omid,
                Omsn = dto.Omsn,
                MainChannel = FullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = RtuPartState.Ok,
                IsReserveChannelSet = FullModel.IsReserveChannelEnabled,
                ReserveChannel = FullModel.IsReserveChannelEnabled
                    ? FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                    : null,
                ReserveChannelState = FullModel.IsReserveChannelEnabled ? RtuPartState.Ok : RtuPartState.NotSetYet,
                OtauNetAddress = dto.OtdrAddress,
                OwnPortCount = dto.OwnPortCount,
                FullPortCount = dto.FullPortCount,
                Serial = dto.Serial,
                Version = dto.Version,
                Version2 = dto.Version2,
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
