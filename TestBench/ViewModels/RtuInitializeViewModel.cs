using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using DirectCharonLibrary;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class RtuInitializeViewModel : Screen
    {
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Serial { get; set; }
        public string PortCount { get; set; }
        public NetAddress OtdrNetAddress { get; set; }

        private RtuPartState _mainChannelState;
        private RtuPartState _reserveChannelState;


        public RtuChannelTestViewModel MainChannelTestViewModel { get; set; }
        public bool IsReserveChannelEnabled { get; set; }
        public RtuChannelTestViewModel ReserveChannelTestViewModel { get; set; }

        private readonly Guid _rtuId;
        private readonly IWindowManager _windowManager;
        private readonly Bus _bus;

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

        public RtuInitializeViewModel(Guid rtuId, ReadModel readModel, IWindowManager windowManager, Bus bus)
        {
            _rtuId = rtuId;
            _windowManager = windowManager;
            _bus = bus;

            var originalRtu = readModel.Rtus.First(r => r.Id == _rtuId);
            Title = originalRtu.Title;
            Comment = originalRtu.Comment;
            Serial = originalRtu.Serial;
            PortCount = $@"{originalRtu.OwnPortCount} / {originalRtu.FullPortCount}";
            OtdrNetAddress = originalRtu.OtdrNetAddress;
            MainChannelTestViewModel = new RtuChannelTestViewModel(originalRtu.MainChannel);
            ReserveChannelTestViewModel = new RtuChannelTestViewModel(originalRtu.ReserveChannel);
            IsReserveChannelEnabled = originalRtu.IsReserveChannelSet;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Network_settings;
        }

        public async Task InitializeRtu()
        {
            // TODO Initialize RTU via Server and RtuManager
            var mainCharon = await RunInitializationProcess();
            if (mainCharon == null)
                return;

            //
            await _bus.SendCommand(ParseInitializationResult(mainCharon));
        }

        private InitializeRtu ParseInitializationResult(Charon mainCharon)
        {
            var cmd = new InitializeRtu
            {
                Id = _rtuId,
                MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                MainChannelState = _mainChannelState,
                ReserveChannel = ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                ReserveChannelState = _reserveChannelState
            };
            cmd.OtdrNetAddress = new NetAddress() { Ip4Address = cmd.MainChannel.Ip4Address, Port = 1500 }; // very old rtu have different otdr address

            cmd.OwnPortCount = mainCharon.OwnPortCount;
            cmd.FullPortCount = mainCharon.FullPortCount;
            cmd.Serial = mainCharon.Serial;

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
                NetAddress = new NetAddress() {Ip4Address = pair.Value.TcpAddress.Ip, Port = pair.Value.TcpAddress.TcpPort},
                PortCount = pair.Value.OwnPortCount,
                FirstPortNumber = pair.Value.StartPortNumber,
                Serial = pair.Value.Serial,
            };
            return otau;
        }

        private async Task<Charon> RunInitializationProcess()
        {
            _mainChannelState = RtuPartState.Normal;
            _reserveChannelState = RtuPartState.None;

            var charonAddress = new TcpAddress(MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address, 23);

            InitilizationProgress = Resources.SID_Please__wait_;
            var mainCharon = new Charon(charonAddress);
            using (new WaitCursor())
            {
                await Task.Run(() => mainCharon.InitializeRtu());
            }

            if (!mainCharon.IsLastCommandSuccessful)
            {
                var vm = new NotificationViewModel(Resources.SID_Error, $@"{mainCharon.LastErrorMessage}");
                _windowManager.ShowDialog(vm);
                InitilizationProgress = Resources.SID_Failed_;
                return null;
            }
            InitilizationProgress = Resources.SID_Successful_;
            return mainCharon;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
