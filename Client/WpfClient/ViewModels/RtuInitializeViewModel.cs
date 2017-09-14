using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Serial { get; set; }
        public string PortCount { get; set; }
        public NetAddress OtdrNetAddress { get; set; }

        public NetAddressTestViewModel MainChannelTestViewModel { get; set; }
        public bool IsReserveChannelEnabled { get; set; }
        public NetAddressTestViewModel ReserveChannelTestViewModel { get; set; }

        private readonly Guid _rtuId;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly Bus _bus;
        private readonly ILogger _log;
        private readonly IniFile _iniFile35;
        private readonly LogFile _logFile;
        private Rtu _originalRtu;

        private string _initilizationProgress;
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
            _rtuId = rtuId;
            _readModel = readModel;
            _windowManager = windowManager;
            _bus = bus;
            _log = log;
            _iniFile35 = iniFile35;
            _logFile = logFile;

            _originalRtu = readModel.Rtus.First(r => r.Id == _rtuId);
            Title = _originalRtu.Title;
            Comment = _originalRtu.Comment;
            Serial = _originalRtu.Serial;
            PortCount = $@"{_originalRtu.OwnPortCount} / {_originalRtu.FullPortCount}";
            OtdrNetAddress = _originalRtu.OtdrNetAddress;
            var serverAddress = _iniFile35.ReadDoubleAddress(11842);
            MainChannelTestViewModel = new NetAddressTestViewModel(_originalRtu.MainChannel, _iniFile35, _logFile, clientId, serverAddress.Main);
            ReserveChannelTestViewModel = new NetAddressTestViewModel(_originalRtu.ReserveChannel, _iniFile35, _logFile, clientId, serverAddress.Reserve);
            IsReserveChannelEnabled = _originalRtu.IsReserveChannelSet;
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as RtuInitializedDto;
            if (dto != null)
            {
               ProcessRtuInitialized(dto);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public void InitializeRtu()
        {
            if (!CheckAddressUniqueness())
                return;

            InitilizationProgress = Resources.SID_Please__wait_;

            var dto = new InitializeRtuDto()
            {
                RtuId = _originalRtu.Id,
                RtuAddresses = new DoubleAddress()
                {
                    Main = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = IsReserveChannelEnabled,
                    Reserve = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                }
            };
            using (new WaitCursor())
            {
                IoC.Get<C2DWcfManager>().InitializeRtu(dto);
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

//            var charonAddress = new NetAddress(MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address, 23);
//            var mainCharon = new Charon(charonAddress, _iniFile35, _logFile);
//            _bus.SendCommand(ParseInitializationResult(mainCharon));
        }

//        private Charon TemporaryFakeInitialization()
        //        {
        //            var charonAddress = new NetAddress(MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address, 23);
        //            var mainCharon = new Charon(charonAddress, _iniFile35, _logFile);
        //            mainCharon.FullPortCount = 8;
        //            mainCharon.OwnPortCount = 8;
        //            mainCharon.Serial = @"1234567";
        //            return mainCharon;
        //        }

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
            if (!(list.Count == 0 || list.Count == 1 && list.First().Id == _rtuId))
            {
                var vm = new NotificationViewModel(Resources.SID_Error, Resources.SID_There_is_RTU_with_the_same_ip_address_);
                _windowManager.ShowDialog(vm);
                return false;
            }
            return true;
        }

       private InitializeRtu ParseInitializationResult(Charon mainCharon)
        {
            var cmd = new InitializeRtu
            {
                Id = _rtuId,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = RtuPartState.Normal,
                IsReserveChannelSet = IsReserveChannelEnabled,
                ReserveChannel = IsReserveChannelEnabled ? ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress() : null,
                ReserveChannelState = IsReserveChannelEnabled ? RtuPartState.Normal : RtuPartState.None,
                OtdrNetAddress = new NetAddress()
                {
                    Ip4Address = mainCharon.NetAddress.Ip4Address,
                    Port = 1500
                }, // TODO very old rtu have different otdr address
                OwnPortCount = mainCharon.OwnPortCount,
                FullPortCount = mainCharon.FullPortCount,
                Serial = mainCharon.Serial,
            };

            foreach (var portCharonPair in mainCharon.Children)
                cmd.Otaus.Add(ParsePortCharonPair(portCharonPair));

            return cmd;
        }

        private AttachOtau ParsePortCharonPair(KeyValuePair<int, Charon> pair)
        {
            var otau = new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = _rtuId,
                MasterPort = pair.Key,
                NetAddress = new NetAddress() {Ip4Address = pair.Value.NetAddress.Ip4Address, Port = pair.Value.NetAddress.Port},
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
