using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly ILifetimeScope _globalScope;

        private readonly TraceStateModelFactory _traceStateModelFactory;
        private readonly OutOfTurnPreciseMeasurementViewModel _outOfTurnPreciseMeasurementViewModel;
        private readonly IWindowManager _windowManager;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly ChildrenViews _childrenViews;

        private List<TraceStateViewModel> LaunchedViews { get; } = new List<TraceStateViewModel>();


        public TraceStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager,
            C2DWcfManager c2DWcfManager, Model readModel, CurrentUser currentUser, 
            ChildrenViews childrenViews, TraceStateModelFactory traceStateModelFactory,
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel)
        {
            _globalScope = globalScope;
            _traceStateModelFactory = traceStateModelFactory;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _childrenViews = childrenViews;

            childrenViews.PropertyChanged += ChildrenViewsPropertyChanged;
        }

        private void ChildrenViewsPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews) sender).ShouldBeClosed)
                {
                    foreach (var traceStateViewModel in LaunchedViews.ToArray())
                    {
                        traceStateViewModel.TryClose();
                        LaunchedViews.Remove(traceStateViewModel);
                    }
                }
            }
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt: AddMeasurement(evnt); return;
                case MeasurementUpdated evnt: UpdateMeasurement(evnt); return;
                case TraceUpdated evnt: UpdateTrace(evnt); return;
                case RtuUpdated evnt: UpdateRtu(evnt); return;
                case ResponsibilitiesChanged evnt: ChangeResponsibility(evnt); return;
            }
        }

        private void AddMeasurement(MeasurementAdded evnt)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == evnt.TraceId);
            if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId)) return;

            var lastMeasurement = _readModel.Measurements.LastOrDefault(m => m.TraceId == evnt.TraceId);
            if (lastMeasurement == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(lastMeasurement);
            Show(traceStateModel, true, false, lastMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent);
        }

        private void UpdateMeasurement(MeasurementUpdated evnt)
        {
            foreach (var viewModel in LaunchedViews)
            {
                if (viewModel.Model.SorFileId == evnt.SorFileId)
                {
                    viewModel.Model.EventStatus = evnt.EventStatus;
                    viewModel.Model.Comment = evnt.Comment;
                }
            }
        }

        private void UpdateTrace(TraceUpdated evnt)
        {
            foreach (var traceStateViewModel in LaunchedViews.Where(m=>m.Model.TraceId == evnt.Id))
            {
                traceStateViewModel.Model.Header.TraceTitle = evnt.Title;
            }
        }

        private void UpdateRtu(RtuUpdated evnt)
        {
            foreach (var traceStateViewModel in LaunchedViews)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == traceStateViewModel.Model.TraceId);
                if (trace.RtuId == evnt.RtuId)
                    traceStateViewModel.Model.Header.RtuTitle = evnt.Title;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void ChangeResponsibility(ResponsibilitiesChanged evnt)
        {
            foreach (var viewModel in LaunchedViews)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == viewModel.Model.TraceId);
                if (!trace.ZoneIds.Contains(_currentUser.ZoneId))
                    viewModel.Close();
            }
        }

        // User clicked on TraceLeaf - State
        public void ShowTraceState(Guid traceId)
        {
            var lastMeasurement = _readModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
            if (lastMeasurement == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(lastMeasurement);
            Show(traceStateModel, true);
        }


        // User clicked on line in TraceStatistics (maybe not on the last line - see parameter)
        public async void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace)
        {
            var sorBytes = await _c2DWcfManager.GetSorBytes(measurement.SorFileId);
            if (sorBytes == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(measurement);
            Show(traceStateModel, isLastMeasurementOnThisTrace);
        }

        // User clicked on line in OpticalEvents (maybe last or not last line, and last event could be not last measurement for this trace) 
        public void ShowTraceState(OpticalEventModel opticalEventModel)
        {
            var traceStateModel = _traceStateModelFactory.CreateModel(opticalEventModel);
            var isLastMeasurementOnThisTrace =
                 IsOpticalEventIsLastMeasurementForTrace(opticalEventModel.TraceId, opticalEventModel.SorFileId);
            Show(traceStateModel, isLastMeasurementOnThisTrace);
        }

        private bool IsOpticalEventIsLastMeasurementForTrace(Guid traceId, int opticalEventSorFileId)
        {
            var measurementEvSo = _readModel.Measurements.LastOrDefault(m => m.TraceId == traceId);
            return measurementEvSo == null || measurementEvSo.SorFileId == opticalEventSorFileId;
        }

        private void Show(TraceStateModel traceStateModel, bool isLastMeasurementOnThisTrace, bool isUserAskedToOpenView = true, bool isTraceStateChanged = false)
        {
            LaunchedViews.RemoveAll(v => !v.IsOpen);

            TraceStateViewModel vm;
            if (isLastMeasurementOnThisTrace)
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateModel.TraceId &&
                                                  (v.Model.SorFileId == traceStateModel.SorFileId ||
                                                    v.IsLastStateForThisTrace));
            }
            else
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateModel.TraceId &&
                                                       (v.Model.SorFileId == traceStateModel.SorFileId));
            }

            var isOutOfTurnVmWaitsThisTrace = _outOfTurnPreciseMeasurementViewModel.IsOpen &&
                     _outOfTurnPreciseMeasurementViewModel.TraceLeaf.Id == traceStateModel.TraceId;

            var mustBeOpen = isUserAskedToOpenView || isTraceStateChanged || isOutOfTurnVmWaitsThisTrace;
            if (vm == null && !mustBeOpen)
                return;

            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(vm);
            }

            if (isOutOfTurnVmWaitsThisTrace)
                _outOfTurnPreciseMeasurementViewModel.TryClose();

            vm = _globalScope.Resolve<TraceStateViewModel>();
            vm.Initialize(traceStateModel, isLastMeasurementOnThisTrace, isTraceStateChanged);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
        }
    }
}