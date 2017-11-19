using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly C2DWcfManager _c2DWcfManager;
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


        public OpticalEventsViewModel(IMyLog logFile, ReadModel readModel, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;

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
                MeasurementId = opticalEvent.MeasurementId,
            });
        }

        public void ChangeRow()
        {
            
            _logFile.AppendLine($"Change row for line {SelectedRow.Nomer}");

        }

        public async void ShowReflectogram()
        {
            var sorbytes = await _c2DWcfManager.GetSorBytesOfMeasurement(SelectedRow.MeasurementId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for line {SelectedRow.Nomer}");
                return;
            }
            var assemblyFilename = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = Path.GetDirectoryName(assemblyFilename) ?? @"c:\";
            var tempFolder = Path.Combine(assemblyFolder, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var sorFilename = Path.Combine(tempFolder, @"meas.sor");
            File.WriteAllBytes(sorFilename, sorbytes);

            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(assemblyFolder, @"..\..\RFTSReflect\reflect.exe");
            process.StartInfo.Arguments = sorFilename;
            process.Start();
        }

    }
}
