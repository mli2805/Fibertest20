using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private TraceStateFilter _selectedTraceStateFilter;
        private EventStatusFilter _selectedEventStatusFilter;
        private OpticalEventVm _selectedRow;

        public string TableTitle { get; set; }
        public ObservableCollection<OpticalEventVm> Rows { get; set; } = new ObservableCollection<OpticalEventVm>();

        public OpticalEventVm SelectedRow
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
            ReflectogramManager reflectogramManager, TraceStateViewsManager traceStateViewsManager)
        {
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;

            InitializeTraceStateFilters();
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
            foreach (var eventStatus in Enum.GetValues(typeof(EventStatus)).OfType<EventStatus>())
            {
                EventStatusFilters.Add(new EventStatusFilter(eventStatus));
            }

            SelectedEventStatusFilter = EventStatusFilters.First();
        }

        private bool OnFilter(object o)
        {
            var opticalEventVm = (OpticalEventVm)o;
            return (SelectedTraceStateFilter.IsOn == false ||
                SelectedTraceStateFilter.TraceState == opticalEventVm.TraceState) &&
                    (SelectedEventStatusFilter.IsOn == false ||
                SelectedEventStatusFilter.EventStatus == opticalEventVm.EventStatus);
        }

        public void AddEvent(Measurement measurement)
        {
            Rows.Add(new OpticalEventVm()
            {
                Nomer = measurement.Id,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                EventRegistrationTimestamp = measurement.EventRegistrationTimestamp,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId)?.Title,
                TraceId = measurement.TraceId,
                TraceTitle = _readModel.Traces.FirstOrDefault(t => t.Id == measurement.TraceId)?.Title,
                BaseRefType = measurement.BaseRefType,

                TraceState = measurement.TraceState,

                EventStatus = measurement.EventStatus,

                StatusChangedTimestamp = measurement.StatusChangedByUser != ""
                    ? measurement.StatusChangedTimestamp.ToString(Thread.CurrentThread.CurrentUICulture)
                    : "",
                StatusChangedByUser = "",
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
    }
}
