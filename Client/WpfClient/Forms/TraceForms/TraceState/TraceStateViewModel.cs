using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly SoundManager _soundManager;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly TraceStateReportProvider _traceStateReportProvider;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly GraphReadModel _graphReadModel;
        private bool _isSoundForThisVmInstanceOn;
        private bool _isTraceStateChanged;
        public bool IsOpen { get; private set; }

        public TraceStateModel Model { get; set; }

        public List<EventStatusComboItem> StatusRows { get; set; }
        public EventStatusComboItem SelectedEventStatus { get; set; }
        public bool HasPrivilegies { get; set; }

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        public TraceStateViewModel(IMyLog logFile, CurrentUser currentUser,
            CurrentlyHiddenRtu currentlyHiddenRtu, ReflectogramManager reflectogramManager,
            SoundManager soundManager, Model readModel,
            IWcfServiceDesktopC2D c2DWcfManager, IWcfServiceInSuperClient c2SWcfManager, 
            CommandLineParameters commandLineParameters, CurrentDatacenterParameters currentDatacenterParameters, 
            TabulatorViewModel tabulatorViewModel, TraceStateReportProvider traceStateReportProvider,
            TraceStatisticsViewsManager traceStatisticsViewsManager, GraphReadModel graphReadModel)
        {
            _logFile = logFile;
            _currentUser = currentUser;
            HasPrivilegies = currentUser.Role <= Role.Operator;
            IsEditEnabled = true;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _reflectogramManager = reflectogramManager;
            _soundManager = soundManager;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _commandLineParameters = commandLineParameters;
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2SWcfManager = c2SWcfManager;
            _tabulatorViewModel = tabulatorViewModel;
            _traceStateReportProvider = traceStateReportProvider;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _graphReadModel = graphReadModel;
        }

        public void Initialize(TraceStateModel model, bool isTraceStateChanged)
        {
            Model = model;
            _isTraceStateChanged = isTraceStateChanged;
            // if (Model.Accidents.Count > 0)
            Model.SelectedAccident = Model.Accidents.FirstOrDefault();
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

        public void ShowAccidentPlace()
        {
            PointLatLng? accidentPoint;
            if (Model.Accidents.Count == 0 || Model.SelectedAccident == null)
                accidentPoint = Model.Header.RtuPosition;
            else accidentPoint = Model.SelectedAccident.Position;

            if (_currentlyHiddenRtu.Collection.Contains(Model.Trace.RtuId))
            {
                //                _renderingManager.ShowOneTrace(Model.Trace);
                _currentlyHiddenRtu.Collection.Remove(Model.Trace.RtuId);
                _currentlyHiddenRtu.ChangedRtu = Model.Trace.RtuId;
            }
            if (accidentPoint != null)
                _graphReadModel.PlacePointIntoScreenCenter((PointLatLng)accidentPoint);
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;

            if (_commandLineParameters.IsUnderSuperClientStart)
            {
                _logFile.AppendLine($@"Ask super-client to switch onto this system (postfix = {_commandLineParameters.ClientOrdinal}).");
                _c2SWcfManager.SwitchOntoSystem(_commandLineParameters.ClientOrdinal);
            }
        }

        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(Model.Header.TraceTitle, Model.SorFileId, Model.MeasurementTimestamp);
            _reflectogramManager.ShowRefWithBase(Model.SorFileId);
        }
        public void ShowRftsEvents() { _reflectogramManager.ShowRftsEvents(Model.SorFileId); }
        public void ShowTraceStatistics() { _traceStatisticsViewsManager.Show(Model.TraceId); }
        public void ExportToKml() { }

        public void ShowHtmlReport()
        {
            var reportModel = CreateReportModelFromMeasurement();
            var htmlFile = EventReport.FillInHtmlReportForTraceState(reportModel);
            try
            {
                Process.Start(htmlFile);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"ShowReport: " + e.Message);
            }
        }
        public void ShowReport()
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string filename = Path.Combine(folder, @"TraceStateReport.pdf");
                var reportModel = new TraceReportModel()
                {
                    TraceTitle = Model.Header.TraceTitle,
                    TraceState = Model.TraceState.ToLocalizedString(),
                    RtuTitle = Model.Header.RtuTitle,
                    RtuSoftwareVersion = Model.Header.RtuSoftwareVersion,
                    PortTitle = Model.Header.PortTitle,
                    MeasurementTimestamp = $@"{Model.MeasurementTimestamp:G}",
                    RegistrationTimestamp = $@"{Model.RegistrationTimestamp:G}",

                    Accidents = Model.Accidents,
                };
                _traceStateReportProvider.Create(reportModel, _currentDatacenterParameters).Save(filename);
                Process.Start(filename);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(@"ShowReport: " + e.Message);
            }
        }

        public async void SaveMeasurementChanges()
        {
            using (new WaitCursor())
            {
                IsEditEnabled = false;
                var dto = new UpdateMeasurement
                {
                    SorFileId = Model.SorFileId,
                    Comment = Model.Comment,
                };

                if (Model.OpticalEventPanelVisibility == Visibility.Visible && Model.EventStatus != SelectedEventStatus.EventStatus)
                {
                    dto.EventStatus = SelectedEventStatus.EventStatus;
                    dto.StatusChangedTimestamp = DateTime.Now;
                    dto.StatusChangedByUser = _currentUser.UserName;
                }
                else
                {
                    dto.EventStatus = Model.EventStatus;
                    var measurement = _readModel.Measurements.FirstOrDefault(m => m.SorFileId == Model.SorFileId);
                    if (measurement != null)
                    {
                        dto.StatusChangedTimestamp = measurement.StatusChangedTimestamp;
                        dto.StatusChangedByUser = measurement.StatusChangedByUser;
                    }
                }

                var result = await _c2DWcfManager.SendCommandAsObj(dto);
                if (result != null)
                    _logFile.AppendLine(@"Cannot update measurement!");
                IsEditEnabled = true;
            }
            TryClose();
        }

        private EventReportModel CreateReportModelFromMeasurement()
        {
            return new EventReportModel()
            {
                TraceTitle = Model.Trace.Title,
                TraceState = Model.TraceState.ToLocalizedString(),
                RtuTitle = Model.Header.RtuTitle,
                Port = Model.Header.PortTitle,
                TimeStamp = Model.MeasurementTimestamp,
            };
        }

        public void Close() { TryClose(); }
    }
}

