﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OtauToAttachViewModel : Screen
    {
        private Guid _rtuId;
        private int _portNumberForAttachment;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

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

        public OtauToAttachViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
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

            NetAddressInputViewModel = new NetAddressInputViewModel(
                //                new NetAddress() {Ip4Address = @"192.168.96.57", Port = 11834, IsAddressSetAsIp = true});
                new NetAddress() { Ip4Address = @"172.16.5.57", Port = 11834, IsAddressSetAsIp = true });
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attach_optical_switch;
        }

        public async Task AttachOtau()
        {
            if (!CheckAddressUniqueness()) return;
          
            using (new WaitCursor())
            {
                AttachmentProgress = Resources.SID_Please__wait_;
                var result = await AttachOtauIntoRtu();
                if (result.IsAttached)
                {
                    AttachmentProgress = Resources.SID_Successful_;
                    var eventSourcingResult = await AttachOtauIntoGraph(result);
                    if (eventSourcingResult == null)
                        UpdateView(result);
                }
                else
                {
                    AttachmentProgress = Resources.SID_Failed_;
                    var vm = new NotificationViewModel(Resources.SID_Error, $@"{result.ErrorMessage}");
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                }
            }
        }

        private async Task<OtauAttachedDto> AttachOtauIntoRtu()
        {
            var otauAddress = new NetAddress(NetAddressInputViewModel.GetNetAddress().Ip4Address,
                NetAddressInputViewModel.GetNetAddress().Port);
            var dto = new AttachOtauDto()
            {
                RtuId = _rtuId,
                OtauId = Guid.NewGuid(),
                OtauAddresses = otauAddress,
                OpticalPort = _portNumberForAttachment
            };
            var result = await _c2DWcfManager.AttachOtauAsync(dto);
            return result;
        }

        private async Task<string> AttachOtauIntoGraph(OtauAttachedDto result)
        {
            var cmd = new AttachOtau()
            {
                Id = result.OtauId,
                RtuId = result.RtuId,
                MasterPort = _portNumberForAttachment,
                Serial = result.Serial,
                PortCount = result.PortCount,
                NetAddress = NetAddressInputViewModel.GetNetAddress(),
            };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private void UpdateView(OtauAttachedDto result)
        {
            OtauSerial = result.Serial;
            OtauPortCount = result.PortCount;
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

        public void Close()
        {
            TryClose();
        }
    }
}
