using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OtauToAttachViewModel : Screen
    {
        private readonly Guid _rtuId;
        private readonly int _portNumberForAttachment;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;

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

        public OtauToAttachViewModel(Guid rtuId, int portNumberForAttachment, ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, IniFile iniFile35, IMyLog logFile)
        {
            _rtuId = rtuId;
            _portNumberForAttachment = portNumberForAttachment;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _iniFile35 = iniFile35;
            _logFile = logFile;

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

            var cmd = new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = _rtuId,
                MasterPort = _portNumberForAttachment,
                Serial = otau.Serial,
                PortCount = otau.OwnPortCount,
                NetAddress = NetAddressInputViewModel.GetNetAddress(),
            };
//            await _bus.SendCommand(cmd);
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public bool CheckAddressUniqueness()
        {
            if (!_readModel.Otaus.Any(o =>
                o.NetAddress.Ip4Address == NetAddressInputViewModel.GetNetAddress().Ip4Address &&
                o.NetAddress.Port == NetAddressInputViewModel.GetNetAddress().Port))
                return true;

            var vm = new NotificationViewModel(Resources.SID_Error, Resources.SID_There_is_optical_switch_with_the_same_tcp_address_);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        private async Task<Charon> OtauAttachProcess()
        {
            AttachmentProgress = Resources.SID_Please__wait_;

            Charon otau = new Charon(
                new NetAddress(NetAddressInputViewModel.GetNetAddress().Ip4Address, NetAddressInputViewModel.GetNetAddress().Port),
                _iniFile35, _logFile);
            using (new WaitCursor())
            {
               await Task.Run(() => RealOtauAttachProcess(otau));
            }

            if (!otau.IsLastCommandSuccessful)
            {
                var vm = new NotificationViewModel(Resources.SID_Error, $@"{otau.LastErrorMessage}");
                _windowManager.ShowDialogWithAssignedOwner(vm);
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
