using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly RtuFilterViewModel _rtuFilterViewModel;
        private readonly IWindowManager _windowManager;
        private TraceStateFilter _selectedTraceStateFilter;
        private EventStatusFilter _selectedEventStatusFilter;
        private OpticalEventModel _selectedRow;

        public string TableTitle { get; set; }
        public ObservableCollection<OpticalEventModel> Rows { get; set; } = new ObservableCollection<OpticalEventModel>();

        public OpticalEventModel SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public List<TraceStateFilter> TraceStateFilters { get; set; }

        public TraceStateFilter SelectedTraceStateFilter
        {
            get { return _selectedTraceStateFilter; }
            set
            {
                if (Equals(value, _selectedTraceStateFilter)) return;
                _selectedTraceStateFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        private RtuGuidFilter _selectedRtuFilter;
        public RtuGuidFilter SelectedRtuFilter
        {
            get { return _selectedRtuFilter; }
            set
            {
                if (Equals(value, _selectedRtuFilter)) return;
                _selectedRtuFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        public List<EventStatusFilter> EventStatusFilters { get; set; }

        public EventStatusFilter SelectedEventStatusFilter
        {
            get { return _selectedEventStatusFilter; }
            set
            {
                if (Equals(value, _selectedEventStatusFilter)) return;
                _selectedEventStatusFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        public OpticalEventsViewModel(ReadModel readModel,
            ReflectogramManager reflectogramManager, TraceStateViewsManager traceStateViewsManager,
            RtuFilterViewModel rtuFilterViewModel, IWindowManager windowManager)
        {
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;
            _rtuFilterViewModel = rtuFilterViewModel;
            _windowManager = windowManager;

            InitializeTraceStateFilters();
            SelectedRtuFilter = new RtuGuidFilter();
            InitializeEventStatusFilters();

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.Filter += OnFilter;
            view.SortDescriptions.Add(new SortDescription(@"Nomer", ListSortDirection.Descending));
        }

        private void InitializeTraceStateFilters()
        {
            TraceStateFilters = new List<TraceStateFilter>() { new TraceStateFilter() };
            TraceStateFilters.Add(new TraceStateFilter(FiberState.Ok));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.Minor));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.Major));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.Critical));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.FiberBreak));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.NoFiber));
            TraceStateFilters.Add(new TraceStateFilter(FiberState.User));

            SelectedTraceStateFilter = TraceStateFilters.First();
        }

        private void InitializeEventStatusFilters()
        {
            EventStatusFilters = new List<EventStatusFilter>() { new EventStatusFilter() };
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Confirmed));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.NotConfirmed));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Planned));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Suspended));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Unprocessed));

            SelectedEventStatusFilter = EventStatusFilters.First();
        }

        private bool OnFilter(object o)
        {
            var opticalEventVm = (OpticalEventModel)o;
            return (SelectedTraceStateFilter.IsOn == false ||
                SelectedTraceStateFilter.TraceState == opticalEventVm.TraceState) &&
                    (SelectedEventStatusFilter.IsOn == false ||
                SelectedEventStatusFilter.EventStatus == opticalEventVm.EventStatus) &&
                    (SelectedRtuFilter.IsOn == false ||
                SelectedRtuFilter.RtuId == opticalEventVm.RtuId);
        }

        public void AddEvent(Measurement measurement)
        {
            Rows.Add(new OpticalEventModel()
            {
                Nomer = measurement.Id,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                EventRegistrationTimestamp = measurement.EventRegistrationTimestamp,
                RtuId = measurement.RtuId,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId)?.Title,
                TraceId = measurement.TraceId,
                TraceTitle = _readModel.Traces.FirstOrDefault(t => t.Id == measurement.TraceId)?.Title,
                BaseRefType = measurement.BaseRefType,
                TraceState = measurement.TraceState,

                EventStatus = measurement.EventStatus,
                StatusChangedTimestamp = measurement.EventStatus.IsStatusAssignedByUser()
                    ? measurement.StatusChangedTimestamp.ToString(Thread.CurrentThread.CurrentUICulture)
                    : "",
                StatusChangedByUser = measurement.EventStatus.IsStatusAssignedByUser() 
                    ? measurement.StatusChangedByUser 
                    : "",

                Comment = measurement.Comment,
                SorFileId = measurement.SorFileId,
            });
        }

        public void RemoveOldEventForTraceIfExists(Guid traceId)
        {
            var oldEvent = Rows.FirstOrDefault(l => l.TraceId == traceId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }

        public void ShowReflectogram(int param)
        {
            if (SelectedRow == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_There_are_no_selected_row_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.SorFileId);
        }

        public void SaveReflectogramAs(bool shouldBaseRefBeExcluded)
        {
            var timestamp = $@"{SelectedRow.EventRegistrationTimestamp:dd-MM-yyyy HH-mm-ss}";
            var defaultFilename = $@"{SelectedRow.TraceTitle} [N{SelectedRow.SorFileId}] {timestamp}";
            _reflectogramManager.SaveReflectogramAs(SelectedRow.SorFileId, defaultFilename, shouldBaseRefBeExcluded);
        }

        public void ShowRftsEvents()
        {
            _reflectogramManager.ShowRftsEvents(SelectedRow.SorFileId);
        }

        public void ShowTraceState()
        {
            _traceStateViewsManager.ShowTraceState(SelectedRow);
        }

        public void ShowRtuFilter()
        {
            _rtuFilterViewModel.Initialize();
            var modalResult = _windowManager.ShowDialogWithAssignedOwner(_rtuFilterViewModel);
            if (modalResult == true)
                SelectedRtuFilter = _rtuFilterViewModel.SelectedRow;
        }

        public void ApplyUsersChanges(UpdateMeasurementDto dto)
        {
            var opticalEventModel = Rows.FirstOrDefault(r => r.SorFileId == dto.SorFileId);
            if (opticalEventModel == null)
                return;

            opticalEventModel.EventStatus = dto.EventStatus;
            opticalEventModel.Comment = dto.Comment;

            Rows.Remove(opticalEventModel);
            Rows.Add(opticalEventModel);
            SelectedRow = opticalEventModel;
        }

        public void RemoveEventsOfTrace(Guid traceId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].TraceId == traceId)
                    Rows.RemoveAt(i);
            }
        }
    }
}
