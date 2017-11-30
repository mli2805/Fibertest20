using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly int _portNumber;
        private readonly OtauPortDto _otauPortDto;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private Trace _selectedTrace;

        public List<Trace> Choices { get; set; }

        public Trace SelectedTrace
        {
            get { return _selectedTrace; }
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceToAttachViewModel(Guid rtuId, int portNumber, OtauPortDto otauPortDto, 
            ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _portNumber = portNumber;
            _otauPortDto = otauPortDto;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            Choices = readModel.Traces.Where(t => t.RtuId == rtuId && t.Port < 1).ToList();
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        public void Attach()
        {
            var cmd = new ReSendBaseRefsDto()
            {
                TraceId = _selectedTrace.Id,
                RtuId = _selectedTrace.RtuId,
                OtauPortDto = _otauPortDto,
            };
            var result = _c2DWcfManager.ReSendBaseRefAsync(cmd).Result;
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                _windowManager.ShowDialog(new NotificationViewModel("Error!", "Cannot send base refs to RTU"));
                return;
            }

            _c2DWcfManager.SendCommandAsObj(new AttachTrace() {Port = _portNumber, TraceId = SelectedTrace.Id, OtauPortDto = _otauPortDto});
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}