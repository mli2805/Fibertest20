using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private OtauPortDto _otauPortDto;
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

        public TraceToAttachViewModel(Model readModel, IWcfServiceForClient c2DWcfManager,
            IWindowManager windowManager, CurrentUser currentUser)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public void Initialize(Guid rtuId, OtauPortDto otauPortDto)
        {
            _otauPortDto = otauPortDto;
            Choices = _readModel.Traces.Where(t => t.RtuId == rtuId && t.Port < 1 && t.ZoneIds.Contains(_currentUser.ZoneId)).ToList();
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        public async void Attach()
        {
            IsButtonsEnabled = false;
            var cmd = new ReSendBaseRefsDto()
            {
                TraceId = _selectedTrace.TraceId,
                RtuId = _selectedTrace.RtuId,
                OtauPortDto = _otauPortDto,
                BaseRefDtos = new List<BaseRefDto>(),
            };
            foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == _selectedTrace.TraceId))
            {
                cmd.BaseRefDtos.Add(new BaseRefDto()
                {
                    SorFileId = baseRef.SorFileId,

                    Id = baseRef.TraceId,
                    BaseRefType = baseRef.BaseRefType,
                    Duration = baseRef.Duration,
                    SaveTimestamp = baseRef.SaveTimestamp,
                    UserName = baseRef.UserName,
                });
            }

            if (cmd.BaseRefDtos.Any())
            {
                var result = await _c2DWcfManager.ReSendBaseRefAsync(cmd);
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                {
                    _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                        Resources.SID_Cannot_send_base_refs_to_RTU));
                    return;
                }
            }

            var command = new AttachTrace()
            {
                TraceId = SelectedTrace.TraceId,
                OtauPortDto = _otauPortDto,
            };

            Measurement measurement =  _readModel.Measurements.LastOrDefault(m=>m.TraceId == _selectedTrace.TraceId);
            if (measurement != null)
            {
                command.PreviousTraceState = measurement.TraceState;
                if (measurement.TraceState != FiberState.Ok &&
                    measurement.TraceState != FiberState.NoFiber)
                    command.AccidentsInLastMeasurement = measurement.Accidents;
            }
            else
            { // trace has no measurements so far 
                command.PreviousTraceState = FiberState.Unknown;
            }

            await _c2DWcfManager.SendCommandAsObj(command);
            IsButtonsEnabled = true;
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}