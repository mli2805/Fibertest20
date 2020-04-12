using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class OtauToAttachViewModel : Screen
    {
        private Guid _rtuId;
        private int _portNumberForAttachment;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;

        public string RtuTitle { get; set; }
        public int RtuPortNumber { get; set; }

        private NetAddressInputViewModel _netAddressInputViewModel;
        public NetAddressInputViewModel NetAddressInputViewModel
        {
            get => _netAddressInputViewModel;
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
            get => _otauSerial;
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
            get => _otauPortCount;
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
            get => _attachmentProgress;
            set
            {
                if (value == _attachmentProgress) return;
                _attachmentProgress = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled = true;
        public bool IsButtonsEnabled
        {
            get { return _isButtonsEnabled; }
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OtauToAttachViewModel(ILifetimeScope globalScope, Model readModel, 
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
        }

        public void Initialize(Guid rtuId, int portNumberForAttachment)
        {
            _rtuId = rtuId;
            _portNumberForAttachment = portNumberForAttachment;
            InitializeView();
        }

        private void InitializeView()
        {
            RtuTitle = _readModel.Rtus.First(r => r.Id == _rtuId).Title;
            RtuPortNumber = _portNumberForAttachment;
            OtauSerial = "";
            OtauPortCount = 0;
            AttachmentProgress = "";

            NetAddressInputViewModel = new NetAddressInputViewModel(
                new NetAddress() { Ip4Address = @"192.168.96.57", Port = 11834, IsAddressSetAsIp = true }, true);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attach_optical_switch;
        }

        public async Task AttachOtau()
        {
            IsButtonsEnabled = false;
            if (!CheckAddressUniqueness())
            {
                IsButtonsEnabled = true;
                return;
            }

            OtauAttachedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                AttachmentProgress = Resources.SID_Please__wait_;
                result = await AttachOtauIntoRtu();
            }
            if (result.IsAttached)
            {
                AttachmentProgress = Resources.SID_Successful_;
                OtauSerial = result.Serial;
                OtauPortCount = result.PortCount;
            }
            else
            {
                AttachmentProgress = Resources.SID_Failed_;
                var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{result.ReturnCode.GetLocalizedString()}");
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }

            IsButtonsEnabled = true;
        }

        private async Task<OtauAttachedDto> AttachOtauIntoRtu()
        {
            var otauAddress = new NetAddress(NetAddressInputViewModel.GetNetAddress().Ip4Address,
                NetAddressInputViewModel.GetNetAddress().Port);
            var dto = new AttachOtauDto()
            {
                RtuId = _rtuId,
                OtauId = Guid.NewGuid(),
                OtauAddress = otauAddress,
                OpticalPort = _portNumberForAttachment
            };
            var result = await _c2RWcfManager.AttachOtauAsync(dto);
            return result;
        }

        private bool CheckAddressUniqueness()
        {
            if (!_readModel.Otaus.Any(o =>
                o.OtauAddress.Ip4Address == NetAddressInputViewModel.GetNetAddress().Ip4Address &&
                o.OtauAddress.Port == NetAddressInputViewModel.GetNetAddress().Port))
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_optical_switch_with_the_same_tcp_address_);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
