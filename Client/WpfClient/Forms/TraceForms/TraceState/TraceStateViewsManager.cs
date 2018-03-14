using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TraceStateModelFactory _traceStateModelFactory;
        private readonly OutOfTurnPreciseMeasurementViewModel _outOfTurnPreciseMeasurementViewModel;
        private readonly IWindowManager _windowManager;
        private readonly C2DWcfManager _c2DWcfManager;
        private readonly ReadModel _readModel;

        private List<TraceStateViewModel> LaunchedViews { get; } = new List<TraceStateViewModel>();


        public TraceStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager,
            C2DWcfManager c2DWcfManager, ReadModel readModel, TraceStateModelFactory traceStateModelFactory, 
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel)
        {
            _globalScope = globalScope;
            _traceStateModelFactory = traceStateModelFactory;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
        }

        // User clicked on TraceLeaf - State
        public void ShowTraceState(Guid traceId)
        {
//            MeasurementWithSor measurementWithSor =  await _c2DWcfManager.GetLastMeasurementForTrace(traceId);
//            if (measurementWithSor == null)
//                return;
//            var traceStateModel = _traceStateModelFactory.CreateModel(measurementWithSor.Measurement, measurementWithSor.SorBytes);
//            Show(traceStateModel, true);
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
        public async void ShowTraceState(OpticalEventModel opticalEventModel)
        {
            var sorBytes = await _c2DWcfManager.GetSorBytes(opticalEventModel.SorFileId);
            if (sorBytes == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(opticalEventModel, sorBytes);
            var isLastMeasurementOnThisTrace =
                 IsOpticalEventIsLastMeasurementForTrace(opticalEventModel.TraceId, opticalEventModel.SorFileId);
            Show(traceStateModel, isLastMeasurementOnThisTrace);
        }

        private bool IsOpticalEventIsLastMeasurementForTrace(Guid traceId, int opticalEventSorFileId)
        {
            var measurementEvSo = _readModel.Measurements.LastOrDefault(m=>m.TraceId == traceId);
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
        }
    }
}