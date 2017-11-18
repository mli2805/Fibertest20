using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;
        private Visibility _opticalEventsVisibility;
        private TraceStateFilter _selectedTraceStateFilter;
        private EventStatusFilter _selectedEventStatusFilter;
        private OpticalEventVm _selectedRow;

        public Visibility OpticalEventsVisibility
        {
            get { return _opticalEventsVisibility; }
            set
            {
                if (value == _opticalEventsVisibility) return;
                _opticalEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

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


        public OpticalEventsViewModel(ReadModel readModel)
        {
            _readModel = readModel;

            InitializeTraceStateFilters();
            InitializeEventStatusFilters();

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.Filter += OnFilter;
            view.SortDescriptions.Add(new SortDescription(@"Nomer",ListSortDirection.Descending));


        }

        private void InitializeTraceStateFilters()
        {
            TraceStateFilters = new List<TraceStateFilter>() {new TraceStateFilter()};
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
            EventStatusFilters = new List<EventStatusFilter>() {new EventStatusFilter()};
            foreach (var eventStatus in Enum.GetValues(typeof(EventStatus)).OfType<EventStatus>())
            {
                EventStatusFilters.Add(new EventStatusFilter(eventStatus));
            }

            SelectedEventStatusFilter = EventStatusFilters.First();
        }

        private bool OnFilter(object o)
        {
            var  opticalEventVm = (OpticalEventVm)o;
            return (SelectedTraceStateFilter.IsOn == false ||
                SelectedTraceStateFilter.TraceState == opticalEventVm.TraceState) &&
                    (SelectedEventStatusFilter.IsOn == false ||
                SelectedEventStatusFilter.EventStatus == opticalEventVm.EventStatus);
        }

        public void Apply(OpticalEvent opticalEvent)
        {
            Rows.Add(new OpticalEventVm()
            {
                Nomer = opticalEvent.Id,
                EventTimestamp = opticalEvent.EventTimestamp,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r=>r.Id == opticalEvent.RtuId)?.Title,
                TraceTitle = _readModel.Traces.FirstOrDefault(t=>t.Id == opticalEvent.TraceId)?.Title,
                BaseRefTypeBrush = 
                    opticalEvent.TraceState == FiberState.Ok 
                        ? Brushes.White 
                        : opticalEvent.BaseRefType == BaseRefType.Fast 
                            ? Brushes.Yellow : opticalEvent.TraceState.GetBrush(),
                TraceState = opticalEvent.TraceState,

                EventStatus = opticalEvent.EventStatus,
                EventStatusBrush = opticalEvent.EventStatus == EventStatus.Confirmed ? Brushes.Red : Brushes.White,

                StatusTimestamp = opticalEvent.EventStatus != EventStatus.NotAnAccident ? opticalEvent.StatusTimestamp.ToString(Thread.CurrentThread.CurrentUICulture) : "",
                StatusUsername = opticalEvent.EventStatus != EventStatus.NotAnAccident ? opticalEvent.StatusUser : "",
                Comment = opticalEvent.Comment,
            });
        }

    }
}
