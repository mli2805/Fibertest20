﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        private readonly Model _readModel;
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
            get => _selectedRow;
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
            get => _selectedTraceStateFilter;
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
            get => _selectedRtuFilter;
            set
            {
                if (Equals(value, _selectedRtuFilter)) return;
                _selectedRtuFilter = value;
                RtuFilterNow = _selectedRtuFilter.ToString();
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        public List<EventStatusFilter> EventStatusFilters { get; set; }

        public EventStatusFilter SelectedEventStatusFilter
        {
            get => _selectedEventStatusFilter;
            set
            {
                if (Equals(value, _selectedEventStatusFilter)) return;
                _selectedEventStatusFilter = value;
                var view = CollectionViewSource.GetDefaultView(Rows);
                view.Refresh();
            }
        }

        private string _rtuFilterNow = @"no filter";
        public string RtuFilterNow
        {
            get => _rtuFilterNow;
            set
            {
                if (value == _rtuFilterNow) return;
                _rtuFilterNow = value;
                NotifyOfPropertyChange();
            }
        }

        public OpticalEventsViewModel(Model readModel,
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

            // TODO: Optimize test suite performance!
            SelectedTraceStateFilter = TraceStateFilters.First();
        }

        private void InitializeEventStatusFilters()
        {
            EventStatusFilters = new List<EventStatusFilter>() { new EventStatusFilter() };
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Confirmed));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.NotConfirmed));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Planned));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.Suspended));
            EventStatusFilters.Add(new EventStatusFilter(EventStatus.NotImportant));
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



        public void RefreshRowsWithUpdatedRtu(Guid rtuId)
        {
            foreach (var opticalEventModel in Rows.Where(m => m.RtuId == rtuId).ToList())
            {
                Rows.Remove(opticalEventModel);
                opticalEventModel.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuId)?.Title;
                Rows.Add(opticalEventModel);
            }
        }

        public void RefreshRowsWithUpdatedTrace(Guid traceId)
        {
            foreach (var opticalEventModel in Rows.Where(m => m.TraceId == traceId).ToList())
            {
                Rows.Remove(opticalEventModel);
                opticalEventModel.TraceTitle = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId)?.Title;
                Rows.Add(opticalEventModel);
            }
        }

        public void AddEvent(Measurement measurement)
        {
            Rows.Add(new OpticalEventModel()
            {
                Nomer = measurement.SorFileId,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                EventRegistrationTimestamp = measurement.EventRegistrationTimestamp,
                RtuId = measurement.RtuId,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId)?.Title,
                TraceId = measurement.TraceId,
                TraceTitle = _readModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId)?.Title,
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
                Accidents = measurement.Accidents,
                SorFileId = measurement.SorFileId,
            });
        }

        // it will be replaced with just arrived event
        public void RemovePreviousEventForTraceIfExists(Guid traceId)
        {
            var oldEvent = Rows.FirstOrDefault(l => l.TraceId == traceId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }

        public void RemoveEventsOfTrace(Guid traceId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].TraceId == traceId)
                    Rows.RemoveAt(i);
            }
        }

        public void UpdateEvent(MeasurementUpdated dto)
        {
            var opticalEventModel = Rows.FirstOrDefault(l => l.SorFileId == dto.SorFileId);
            if (opticalEventModel == null) return;

            Rows.Remove(opticalEventModel);

            opticalEventModel.EventStatus = dto.EventStatus;
            opticalEventModel.StatusChangedByUser = dto.StatusChangedByUser;
            opticalEventModel.StatusChangedTimestamp = dto.StatusChangedTimestamp.ToString(CultureInfo.CurrentCulture);
            opticalEventModel.Comment = dto.Comment;

            Rows.Add(opticalEventModel);
        }

        public void ShowReflectogram(int param)
        {
            if (SelectedRow == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_There_are_no_selected_row_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            _reflectogramManager.SetTempFileName(SelectedRow.TraceTitle, SelectedRow.Nomer, SelectedRow.EventRegistrationTimestamp);
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.SorFileId);
        }

        public void SaveReflectogramAs(bool shouldBaseRefBeExcluded)
        {
            _reflectogramManager.SetTempFileName(SelectedRow.TraceTitle, SelectedRow.SorFileId, SelectedRow.EventRegistrationTimestamp);
            _reflectogramManager.SaveReflectogramAs(SelectedRow.SorFileId, shouldBaseRefBeExcluded);
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

        public void ApplyUsersChanges(UpdateMeasurement dto)
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


    }
}
