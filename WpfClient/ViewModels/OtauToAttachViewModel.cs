using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Client
{
    public class OtauToAttachViewModel : Screen
    {
        private readonly Guid _rtuId;
        private readonly int _portNumberForAttachment;
        private readonly ReadModel _readModel;
        private readonly Bus _bus;
        private readonly IWindowManager _windowManager;
        private readonly Logger35 _logger35;

        public string RtuTitle { get; set; }
        public int RtuPortNumber { get; set; }

        private NetAddressInputViewModel _netAddressInputViewModel;
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

        private string _otauSerial;
        public string OtauSerial
        {
            get { return _otauSerial; }
            set
            {
                if (value == _otauSerial) return;
                _otauSerial = value;
                NotifyOfPropertyChange();
            }
        }

        private int _otauPortCount;
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

        private string _attachmentProgress;
        public string AttachmentProgress
        {
            get { return _attachmentProgress; }
            set
            {
                if (value == _attachmentProgress) return;
                _attachmentProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public OtauToAttachViewModel(Guid rtuId, int portNumberForAttachment, ReadModel readModel, Bus bus, IWindowManager windowManager, Logger35 logger35)
        {
            _rtuId = rtuId;
            _portNumberForAttachment = portNumberForAttachment;
            _readModel = readModel;
            _bus = bus;
            _windowManager = windowManager;
            _logger35 = logger35;

            InitializeView();
        }

        private void InitializeView()
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

        public async Task AttachOtau()
        {
            if (!CheckAddressUniqueness())
                return;

            var otau = await OtauAttachProcess();
            if (otau == null)
                return;

            await _bus.SendCommand(new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = _rtuId,
                MasterPort = _portNumberForAttachment,
                Serial = otau.Serial,
                PortCount = otau.OwnPortCount,
                NetAddress = NetAddressInputViewModel.GetNetAddress(),
                FirstPortNumber = _readModel.Rtus.First(r => r.Id == _rtuId).FullPortCount,
            });
        }

        public bool CheckAddressUniqueness()
        {
            if (!_readModel.Otaus.Any(o =>
                o.NetAddress.Ip4Address == NetAddressInputViewModel.GetNetAddress().Ip4Address &&
                o.NetAddress.Port == NetAddressInputViewModel.GetNetAddress().Port))
                return true;

            var vm = new NotificationViewModel(Resources.SID_Error, Resources.SID_There_is_optical_switch_with_the_same_tcp_address_);
            _windowManager.ShowDialog(vm);
            return false;
        }

        private async Task<Charon> OtauAttachProcess()
        {
            AttachmentProgress = Resources.SID_Please__wait_;

            Charon otau = new Charon(new NetAddress(NetAddressInputViewModel.GetNetAddress().Ip4Address, NetAddressInputViewModel.GetNetAddress().Port), _logger35, CharonLogLevel.PublicCommands);
            using (new WaitCursor())
            {
               await Task.Run(() => RealOtauAttachProcess(otau));
            }

            if (!otau.IsLastCommandSuccessful)
            {
                var vm = new NotificationViewModel(Resources.SID_Error, $@"{otau.LastErrorMessage}");
                _windowManager.ShowDialog(vm);
                AttachmentProgress = Resources.SID_Failed_;
                return null;
            }

            AttachmentProgress = Resources.SID_Successful_;
            return otau;
        }

        private Charon RealOtauAttachProcess(Charon otau)
        {
            //TODO really attach otau
            Thread.Sleep(1000);
            otau.Serial = @"123456";
            otau.OwnPortCount = 16;
            return otau;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
