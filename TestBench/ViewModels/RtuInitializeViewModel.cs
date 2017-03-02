using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class RtuInitializeViewModel : Screen
    {
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Serial { get; set; }
        public string PortCount { get; set; }

        private RtuPartState _mainChannelState;
        private RtuPartState _reserveChannelState;
        private int _ownPortCount;
        private int _fullPortCount;
        private string _serial;

        private readonly Guid _rtuId;
        private readonly Rtu _originalRtu;
        private readonly ReadModel _readModel;
        private readonly Bus _bus;

        public RtuInitializeViewModel(Guid rtuId, ReadModel readModel, Bus bus)
        {
            _rtuId = rtuId;
            _readModel = readModel;
            _bus = bus;

            _originalRtu = _readModel.Rtus.First(r => r.Id == _rtuId);
            Title = _originalRtu.Title;
            Comment = _originalRtu.Comment;
            Serial = _originalRtu.Serial;
            PortCount = $@"{_originalRtu.OwnPortCount} / {_originalRtu.FullPortCount}";
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "RTU Settings";
        }

        public void Initialize()
        {
            var cmd = new InitializeRtu() {Id = _rtuId};

            // TODO Real Initialize RTU
            RunInitializeProcess();
            //

            cmd.MainChannelState = _mainChannelState;
            cmd.ReserveChannelState = _reserveChannelState;
            cmd.OwnPortCount = _ownPortCount;
            cmd.FullPortCount = _fullPortCount;
            cmd.Serial = _serial;

            _bus.SendCommand(cmd);
            TryClose();
        }

        private void RunInitializeProcess()
        {
            _mainChannelState = RtuPartState.Normal;
            _reserveChannelState = RtuPartState.None;
            _ownPortCount = 8;
            _fullPortCount = 8;
            _serial = @"85615";
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
