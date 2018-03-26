﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly SoundManager _soundManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private bool _isSoundForThisVmInstanceOn;
        private bool _isTraceStateChanged;
        public bool IsOpen { get; private set; }

        public TraceStateModel Model { get; set; }
        public bool IsLastStateForThisTrace { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }

        public TraceStateViewModel(IMyLog logFile, CurrentUser currentUser, ReflectogramManager reflectogramManager, 
            SoundManager soundManager, IWcfServiceForClient c2DWcfManager, 
            TraceStatisticsViewsManager traceStatisticsViewsManager)
        {
            _logFile = logFile;
            _currentUser = currentUser;
            _reflectogramManager = reflectogramManager;
            _soundManager = soundManager;
            _c2DWcfManager = c2DWcfManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
        }

        public void Initialize(TraceStateModel model, bool isLastStateForThisTrace, bool isTraceStateChanged)
        {
            Model = model;
            IsLastStateForThisTrace = isLastStateForThisTrace;
            _isTraceStateChanged = isTraceStateChanged;
            if (Model.EventStatus > EventStatus.EventButNotAnAccident)
                InitializeEventStatusCombobox();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_state;
            IsOpen = true;

            if (!_isTraceStateChanged)
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
                new EventStatusComboItem() {EventStatus = EventStatus.NotImportant},
                new EventStatusComboItem() {EventStatus = EventStatus.Unprocessed},
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

        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(Model.Header.TraceTitle, Model.SorFileId, Model.MeasurementTimestamp);
            _reflectogramManager.ShowRefWithBase(Model.SorFileId);
        }
        public void ShowRftsEvents() { _reflectogramManager.ShowRftsEvents(Model.SorFileId); }
        public void ShowTraceStatistics() { _traceStatisticsViewsManager.Show(Model.TraceId); }
        public void ExportToKml() { }
        public void ShowReport() { }

        public async void SaveMeasurementChanges()
        {
            using (new WaitCursor())
            {
                var dto = new UpdateMeasurement
                {
                    SorFileId = Model.SorFileId,
                    Comment = Model.Comment,
                    EventStatus = Model.OpticalEventPanelVisibility == Visibility.Visible
                        ? SelectedEventStatus.EventStatus
                        : Model.EventStatus,
                    StatusChangedTimestamp = DateTime.Now,
                    StatusChangedByUser = _currentUser.UserName,
                };

                var result = await _c2DWcfManager.SendCommandAsObj(dto);
                if (result != null)
                    _logFile.AppendLine(@"Cannot update measurement!");
              
            }
            TryClose();
        }

        public void Close() { TryClose(); }
    }
}
