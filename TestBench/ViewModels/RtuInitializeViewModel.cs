using System;
using System.Linq;
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
            DisplayName = Resources.SID_RTU_Settings;
        }

        public void InitializeRtu()
        {
            // TODO Initialize RTU via Server and RtuManager
            var mainCharon = RunInitializationProcess();
            if (mainCharon == null)
                return;
            //

            var cmd = new InitializeRtu() {Id = _rtuId};
            cmd.MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress();
            cmd.MainChannelState = _mainChannelState;
            cmd.ReserveChannel = ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress();
            cmd.ReserveChannelState = _reserveChannelState;
            cmd.OtdrNetAddress = new NetAddress() {Ip4Address = cmd.MainChannel.Ip4Address, Port = 1500}; // very old rtu have different otdr address

            cmd.OwnPortCount = mainCharon.OwnPortCount;
            cmd.FullPortCount = mainCharon.FullPortCount;
            cmd.Serial = mainCharon.Serial;

            _bus.SendCommand(cmd);
            TryClose();
        }

        private Charon RunInitializationProcess()
        {
            _mainChannelState = RtuPartState.Normal;
            _reserveChannelState = RtuPartState.None;

            var charonAddress = new TcpAddress(MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address, 23);
            var mainCharon = new Charon(charonAddress);
            if (!mainCharon.Initialize())
            {
                var vm = new NotificationViewModel(Resources.SID_Error, $@"{mainCharon.LastErrorMessage}");
                _windowManager.ShowDialog(vm);
                return null;
            }
            return mainCharon;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
