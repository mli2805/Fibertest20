using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly SoundManager _soundManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private bool _isSoundForThisVmInstanceOn;
        private bool _isUserAskedToOpenView;
        public bool IsOpen { get; set; }

        public TraceStateVm Model { get; set; }
        public bool IsLastStateForThisTrace { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }

        public TraceStateViewModel(IMyLog logFile, ReflectogramManager reflectogramManager, 
            SoundManager soundManager, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _reflectogramManager = reflectogramManager;
            _soundManager = soundManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initialize(TraceStateVm model, bool isLastStateForThisTrace, bool isUserAskedToOpenView)
        {
            Model = model;
            IsLastStateForThisTrace = isLastStateForThisTrace;
            _isUserAskedToOpenView = isUserAskedToOpenView;
            if (Model.EventStatus != EventStatus.NotAnAccident)
                InitializeEventStatusCombobox();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_state;
            IsOpen = true;

            if (!_isUserAskedToOpenView)
                return;

            if (Model.TraceState != FiberState.Ok)
            {
                _isSoundForThisVmInstanceOn = true;
                _soundManager.StartAlert();
                Model.IsSoundButtonEnabled = true;
            }
            else
            {
                _soundManager.PlayOk();
            }
        }

        private void InitializeEventStatusCombobox()
        {
            StatusRows = new List<EventStatusComboItem>
            {  // not foreach because order matters
                new EventStatusComboItem() {EventStatus = EventStatus.Confirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.NotConfirmed},
                new EventStatusComboItem() {EventStatus = EventStatus.Planned},
                new EventStatusComboItem() {EventStatus = EventStatus.Suspended},
                new EventStatusComboItem() {EventStatus = EventStatus.Unprocessed}
            };

            SelectedEventStatus = StatusRows.FirstOrDefault(r => r.EventStatus == Model.EventStatus);
        }


        //----
        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            _isSoundForThisVmInstanceOn = false;
            Model.IsSoundButtonEnabled = false;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            IsOpen = false;
            callback(true);
        }

        public void ShowAccidentPlace() { }
        public void ShowReflectogram() { _reflectogramManager.ShowRefWithBase(Model.SorFileId); }
        public void ShowRftsEvents() { }
        public void ShowTraceStatistics() { }
        public void ExportToKml() { }
        public void ShowReport() { }

        public async void SaveMeasurementChanges()
        {
            var dto = new UpdateMeasurementDto()
            {
                SorFileId = Model.SorFileId,
                EventStatus = SelectedEventStatus.EventStatus,
                Comment = Model.Comment,
            };

            var result = await _c2DWcfManager.SaveMeasurementChanges(dto);
            if (result.ReturnCode != ReturnCode.Ok)
            {
                _logFile.AppendLine(@"Cannot update measurement!");
            }
            TryClose();
        }

        public void Cancel() { TryClose(); }
    }
}
