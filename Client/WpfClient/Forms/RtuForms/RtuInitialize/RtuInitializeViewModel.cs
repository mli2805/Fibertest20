﻿using System;
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
            get { return _originalRtu; }
            set
            {
                if (Equals(value, _originalRtu)) return;
                _originalRtu = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OtdrAddress));
            }
        }

        public string OtdrAddress => OriginalRtu.OtdrNetAddress.Ip4Address == @"192.168.88.101" // fake address on screen
            ? OriginalRtu.MainChannel.Ip4Address
            : OriginalRtu.OtdrNetAddress.Ip4Address;


        public RtuInitializeViewModel(ILifetimeScope globalScope, Model readModel, IWindowManager windowManager,
            IWcfServiceForClient c2DWcfManager, IMyLog logFile, RtuLeaf rtuLeaf, CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
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
        }

        private void ReserveChannelTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
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
            if (e.PropertyName == "Result")
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
            if (string.IsNullOrEmpty(OriginalRtu.Title))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Title_should_be_set_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            if (!CheckAddressUniqueness())
                return;

            var dto = new InitializeRtuDto()
            {
                RtuId = OriginalRtu.Id,

                Serial = OriginalRtu.Serial,         // properties after previous initialization (if it was)
                OwnPortCount = OriginalRtu.OwnPortCount,
                Children = OriginalRtu.Children,

                RtuAddresses = new DoubleAddress()
                {
                    Main = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = IsReserveChannelEnabled,
                    Reserve = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                },
                ShouldMonitoringBeStopped = OriginalRtu.OwnPortCount == 0, // if it's first initialization for this RTU - monitoring should be stopped - in case it's running somehow
            };
            RtuInitializedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                if (!await CheckConnectionBeforeInitializaion()) return;

                _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_RTU_is_being_initialized___;
                result = await _c2DWcfManager.InitializeRtuAsync(dto);
                _commonStatusBarViewModel.StatusBarMessage2 = "";
            }
            ProcessRtuInitialized(result);
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

            ShowInitializationResultMessageBox(dto);

            if (!dto.IsInitialized)
                return;

            // apply initialization to graph
            _c2DWcfManager.SendCommandAsObj(ParseInitializationResult(dto));
            ShowNewRtuInfo(dto);
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

        private void ShowNewRtuInfo(RtuInitializedDto dto)
        {
            var originalRtu1 = new Rtu
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
            OriginalRtu = originalRtu1;
        }

        public bool CheckAddressUniqueness()
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
            if (!(list.Count == 0 || list.Count == 1 && list.First().Id == OriginalRtu.Id))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_RTU_with_the_same_ip_address_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            return true;
        }

        private InitializeRtu ParseInitializationResult(RtuInitializedDto dto)
        {
            var cmd = new InitializeRtu
            {
                Id = dto.RtuId,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = RtuPartState.Ok,
                IsReserveChannelSet = IsReserveChannelEnabled,
                ReserveChannel = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                ReserveChannelState = IsReserveChannelEnabled ? RtuPartState.Ok : RtuPartState.NotSetYet,
                OtauNetAddress = dto.OtdrAddress,
                OwnPortCount = dto.OwnPortCount,
                FullPortCount = dto.FullPortCount,
                Serial = dto.Serial,
                Version = dto.Version,
                IsMonitoringOn = dto.IsMonitoringOn,
                AcceptableMeasParams = dto.AcceptableMeasParams,
            };

            foreach (var portCharonPair in dto.Children)
                cmd.Otaus.Add(ParsePortCharonPair(portCharonPair));

            return cmd;
        }

        private AttachOtau ParsePortCharonPair(KeyValuePair<int, OtauDto> pair)
        {
            var otau = new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = OriginalRtu.Id,
                MasterPort = pair.Key,
                NetAddress = pair.Value.NetAddress,
                PortCount = pair.Value.OwnPortCount,
                Serial = pair.Value.Serial,
            };
            return otau;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
