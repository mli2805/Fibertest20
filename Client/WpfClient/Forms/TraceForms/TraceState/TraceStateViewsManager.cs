﻿using System;
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
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly ChildrenViews _childrenViews;

        private List<TraceStateViewModel> LaunchedViews { get; } = new List<TraceStateViewModel>();

        public TraceStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager,
            CommonC2DWcfManager commonC2DWcfManager, Model readModel, CurrentUser currentUser,
            ChildrenViews childrenViews, TraceStateModelFactory traceStateModelFactory,
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel)
        {
            _globalScope = globalScope;
            _traceStateModelFactory = traceStateModelFactory;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _windowManager = windowManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _childrenViews = childrenViews;

            childrenViews.PropertyChanged += ChildrenViewsPropertyChanged;
        }

        private void ChildrenViewsPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews)sender).ShouldBeClosed)
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

            var traceStateModel = _traceStateModelFactory.CreateModel(lastMeasurement, true, lastMeasurement.TraceState != FiberState.Ok);
            Show(traceStateModel, false, lastMeasurement.EventStatus > EventStatus.JustMeasurementNotAnEvent);
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
            foreach (var traceStateViewModel in LaunchedViews.Where(m => m.Model.TraceId == evnt.Id))
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

            var traceStateModel = _traceStateModelFactory.CreateModel(lastMeasurement, true, lastMeasurement.TraceState != FiberState.Ok);
            Show(traceStateModel);
        }


        // User clicked on line in TraceStatistics (maybe not on the last line - see parameter)
        public async void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace, bool isLastAccident)
        {
            var sorBytes = await _commonC2DWcfManager.GetSorBytes(measurement.SorFileId);
            if (sorBytes == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(measurement, isLastMeasurementOnThisTrace, isLastAccident);
            Show(traceStateModel);
        }

        // User clicked on line in OpticalEvents (maybe last or not last line, and last event could be not last measurement for this trace) 
        public void ShowTraceState(OpticalEventModel opticalEventModel, bool isLastMeasurementOnThisTrace, bool isLastAccident)
        {
            var traceStateModel = _traceStateModelFactory.CreateModel(opticalEventModel, isLastMeasurementOnThisTrace, isLastAccident);
            if (traceStateModel != null)
                Show(traceStateModel);
        }

        private void Show(TraceStateModel traceStateModel, bool isUserAskedToOpenView = true, bool isTraceStateChanged = false)
        {
            LaunchedViews.RemoveAll(v => !v.IsOpen);

            TraceStateViewModel vm;
            if (traceStateModel.IsLastStateForThisTrace)
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateModel.TraceId &&
                                                  (v.Model.SorFileId == traceStateModel.SorFileId ||
                                                    v.Model.IsLastStateForThisTrace));
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
            vm.Initialize(traceStateModel, isTraceStateChanged);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
        }
    }
}