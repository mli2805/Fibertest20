﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Serilog;
using WcfConnections;

namespace Iit.Fibertest.Client
{
    public class RtuInitializeViewModel : Screen
    {
        public Guid RtuId { get; set; }
        public string Comment { get; set; }

        public string Serial
        {
            get { return _serial; }
            set
            {
                if (value == _serial) return;
                _serial = value;
                NotifyOfPropertyChange();
            }
        }

        public string PortCount
        {
            get { return _portCount; }
            set
            {
                if (value == _portCount) return;
                _portCount = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddress OtdrNetAddress
        {
            get { return _otdrNetAddress; }
            set
            {
                if (Equals(value, _otdrNetAddress)) return;
                _otdrNetAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddressTestViewModel MainChannelTestViewModel { get; set; }
        public bool IsReserveChannelEnabled { get; set; }
        public NetAddressTestViewModel ReserveChannelTestViewModel { get; set; }

        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly Bus _bus;
        private readonly ILogger _log;
        private readonly IniFile _iniFile35;
        private readonly LogFile _logFile;

        public Rtu OriginalRtu
        {
            get { return _originalRtu; }
            set
            {
                if (Equals(value, _originalRtu)) return;
                _originalRtu = value;
                NotifyOfPropertyChange();
            }
        }

        private string _initilizationProgress;
        private string _serial;
        private string _portCount;
        private NetAddress _otdrNetAddress;
        private Rtu _originalRtu;

        public string InitilizationProgress
        {
            get { return _initilizationProgress; }
            set
            {
                if (value == _initilizationProgress) return;
                _initilizationProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuInitializeViewModel(Guid clientId, Guid rtuId,
            ReadModel readModel, IWindowManager windowManager, Bus bus, IniFile iniFile35, ILogger log, LogFile logFile)
        {
            RtuId = rtuId;
            _readModel = readModel;
            _windowManager = windowManager;
            _bus = bus;
            _log = log;
            _iniFile35 = iniFile35;
            _logFile = logFile;

            OriginalRtu = readModel.Rtus.First(r => r.Id == RtuId);
            Comment = OriginalRtu.Comment;
            Serial = OriginalRtu.Serial;
            PortCount = $@"{OriginalRtu.OwnPortCount} / {OriginalRtu.FullPortCount}";
            OtdrNetAddress = OriginalRtu.OtdrNetAddress;
            var serverAddress = _iniFile35.ReadDoubleAddress(11842);
            MainChannelTestViewModel = new NetAddressTestViewModel(OriginalRtu.MainChannel, _iniFile35, _logFile, clientId, serverAddress.Main);
            ReserveChannelTestViewModel = new NetAddressTestViewModel(OriginalRtu.ReserveChannel, _iniFile35, _logFile, clientId, serverAddress.Reserve);
            IsReserveChannelEnabled = OriginalRtu.IsReserveChannelSet;
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as RtuInitializedDto;
            if (dto != null)
            {
                ProcessRtuInitialized(dto);
            }

            var dto0 = e as RtuCommandDeliveredDto;
            if (dto0 != null)
            {
                if (dto0.RtuId != RtuId)
                    return;
                if (dto0.MessageProcessingResult == MessageProcessingResult.FailedToTransmit)
                {
                    InitilizationProgress = Resources.SID_Can_t_establish_connection_with_RTU_;
                    MessageBox.Show(
                        string.Format(Resources.SID_Can_t_establish_connection_with_RTU___0_Press_Test_before_Initialization_, Environment.NewLine),
                        Resources.SID_Error);
                }
                else if (dto0.MessageProcessingResult == MessageProcessingResult.TransmittedSuccessfully)
                {
                    InitilizationProgress = string.Format(Resources.SID_Command_is_transmitted_to_RTU___0_Confirmation_is_waited_, Environment.NewLine);
                }

            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public void InitializeRtu()
        {
            if (string.IsNullOrEmpty(OriginalRtu.Title))
            {
                MessageBox.Show(Resources.SID_Title_should_be_set_, Resources.SID_Error);
                return;
            }
            if (!CheckAddressUniqueness())
                return;

            InitilizationProgress = Resources.SID_Please__wait_;

            var dto = new InitializeRtuDto()
            {
                RtuId = OriginalRtu.Id,
                RtuAddresses = new DoubleAddress()
                {
                    Main = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = IsReserveChannelEnabled,
                    Reserve = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                }
            };
            using (new WaitCursor())
            {
                if (!IoC.Get<C2DWcfManager>().InitializeRtu(dto))
                {
                    InitilizationProgress = Resources.SID_Can_t_establish_connection_with_server_;
                    MessageBox.Show(Resources.SID_Can_t_establish_connection_with_server_, Resources.SID_Error);
                }
            }
        }

        private void ProcessRtuInitialized(RtuInitializedDto dto)
        {
            if (!dto.IsInitialized)
            {
                var vm = new NotificationViewModel(Resources.SID_Error, @"RTU is not initialized");
                _windowManager.ShowDialog(vm);
                _log.Information(@"RTU is not initialized");
                InitilizationProgress = Resources.SID_Failed_;
                return;
            }
            _log.Information(@"RTU initialized successfully!");
            InitilizationProgress = Resources.SID_Successful_;

            OriginalRtu.OtdrNetAddress = (NetAddress)dto.OtdrAddress.Clone();
            OriginalRtu.Serial = dto.Serial;
            OriginalRtu.OwnPortCount = dto.OwnPortCount;
            OriginalRtu.FullPortCount = dto.FullPortCount;
            OriginalRtu.RtuManagerSoftwareVersion = dto.Version;
            OriginalRtu.Children = dto.Children;

            _bus.SendCommand(ParseInitializationResult(dto));
        }

        private Charon TemporaryFakeInitialization()
        {
            var charonAddress = new NetAddress(MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address, 23);
            var mainCharon = new Charon(charonAddress, _iniFile35, _logFile);
            mainCharon.FullPortCount = 8;
            mainCharon.OwnPortCount = 8;
            mainCharon.Serial = @"1234567";
            return mainCharon;
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
            if (!(list.Count == 0 || list.Count == 1 && list.First().Id == RtuId))
            {
                var vm = new NotificationViewModel(Resources.SID_Error, Resources.SID_There_is_RTU_with_the_same_ip_address_);
                _windowManager.ShowDialog(vm);
                return false;
            }
            return true;
        }

        private InitializeRtu ParseInitializationResult(RtuInitializedDto dto)
        {
            var cmd = new InitializeRtu
            {
                Id = RtuId,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = RtuPartState.Normal,
                IsReserveChannelSet = IsReserveChannelEnabled,
                ReserveChannel = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                ReserveChannelState = IsReserveChannelEnabled ? RtuPartState.Normal : RtuPartState.None,
                OtdrNetAddress = dto.OtdrAddress,
                OwnPortCount = dto.OwnPortCount,
                FullPortCount = dto.FullPortCount,
                Serial = dto.Serial,
                Version = dto.Version,
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
                RtuId = RtuId,
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
