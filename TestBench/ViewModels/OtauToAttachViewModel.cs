using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class OtauToAttachViewModel : Screen
    {
        private readonly Guid _rtuId;
        private readonly int _portNumberForAttachment;
        private readonly ReadModel _readModel;
        private readonly Bus _bus;
        private readonly IWindowManager _windowManager;
        private int _otauSerial;
        private int _otauPortCount;
        private NetAddressInputViewModel _netAddressInputViewModel;

        public string RtuTitle { get; set; }
        public int RtuPortNumber { get; set; }

        public NetAddressInputViewModel NetAddressInputViewModel
        {
            get { return _netAddressInputViewModel; }
            set
            {
                if (Equals(value, _netAddressInputViewModel)) return;
                _netAddressInputViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public int OtauSerial
        {
            get { return _otauSerial; }
            set
            {
                if (value == _otauSerial) return;
                _otauSerial = value;
                NotifyOfPropertyChange();
            }
        }

        public int OtauPortCount
        {
            get { return _otauPortCount; }
            set
            {
                if (value == _otauPortCount) return;
                _otauPortCount = value;
                NotifyOfPropertyChange();
            }
        }

        public OtauToAttachViewModel(Guid rtuId, int portNumberForAttachment, ReadModel readModel, Bus bus, IWindowManager windowManager)
        {
            _rtuId = rtuId;
            _portNumberForAttachment = portNumberForAttachment;
            _readModel = readModel;
            _bus = bus;
            _windowManager = windowManager;

            Initialize();
        }

        private void Initialize()
        {
            RtuTitle = _readModel.Rtus.First(r => r.Id == _rtuId).Title;
            RtuPortNumber = _portNumberForAttachment;

            NetAddressInputViewModel = new NetAddressInputViewModel(
                new NetAddress() {Ip4Address = @"192.168.96.57", Port = 11834, IsAddressSetAsIp = true});
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attach_optical_switch;
        }

        public void Attach()
        {
            if (!RealActionsToAttachOtau())
            {
                var vm = new NotificationViewModel(Resources.SID_Error, Resources.SID_Optical_switch_attach_error);
                _windowManager.ShowDialog(vm);
                TryClose();
            }

            _bus.SendCommand(new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = _rtuId,
                MasterPort = _portNumberForAttachment,
                Serial = OtauSerial,
                PortCount = OtauPortCount,
                NetAddress = NetAddressInputViewModel.GetNetAddress(),
                FirstPortNumber = _readModel.Rtus.First(r => r.Id == _rtuId).FullPortCount,
            });
        }

        public void Close()
        {
            TryClose();
        }

        private bool RealActionsToAttachOtau()
        {
            //TODO attach otau

            OtauSerial = 123456;
            OtauPortCount = 16;

            return true;
        }
    }
}
