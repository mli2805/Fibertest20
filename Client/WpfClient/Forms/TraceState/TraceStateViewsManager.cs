using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly TraceStateVmFactory _traceStateVmFactory;
        private readonly IWindowManager _windowManager;
        private readonly C2DWcfManager _c2DWcfManager;

        private List<TraceStateViewModel> LaunchedViews { get; set; } = new List<TraceStateViewModel>();


        public TraceStateViewsManager(TraceStateVmFactory traceStateVmFactory,
            IWindowManager windowManager, C2DWcfManager c2DWcfManager)
        {
            _traceStateVmFactory = traceStateVmFactory;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        // User clicked on TraceLeaf - State
        public async void ShowTraceState(Guid traceId)
        {
            var traceStateVm = await _traceStateVmFactory.Create(traceId);
            Show(traceStateVm, true);
        }

        // MonitoringResult arrived by Wcf
        public void NotifyAboutMonitoringResult(Measurement measurement)
        {
            var traceStateVm = _traceStateVmFactory.CreateVm(measurement);
            Show(traceStateVm, 
                isLastMeasurementOnThisTrace: true, 
                isUserAskedToOpenView: false, 
                isTraceStateChanged: measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent);
        }

        // User clicked on line in TraceStatistics (maybe not on the last line - see parameter)
        public void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace)
        {
            var traceStateVm = _traceStateVmFactory.CreateVm(measurement);
            Show(traceStateVm, isLastMeasurementOnThisTrace);
        }

        // User clicked on line in OpticalEvents (maybe last or not last line, and last event could be not last measurement for this trace) 
        public async void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            var lastMeasurement = await _c2DWcfManager.GetLastMeasurementForTrace(opticalEventVm.TraceId);
            if (lastMeasurement == null)
                return;

            var traceStateVm = _traceStateVmFactory.CreateVm(opticalEventVm);

            Show(traceStateVm, lastMeasurement.SorFileId == opticalEventVm.SorFileId);
        }


        private void Show(TraceStateVm traceStateVm, bool isLastMeasurementOnThisTrace, bool isUserAskedToOpenView = true, bool isTraceStateChanged = false)
        {
            LaunchedViews.RemoveAll(v => !v.IsOpen);

            TraceStateViewModel vm;
            if (isLastMeasurementOnThisTrace)
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateVm.TraceId &&
                                                  (v.Model.SorFileId == traceStateVm.SorFileId ||
                                                    v.IsLastStateForThisTrace));
            }
            else
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateVm.TraceId &&
                                                       (v.Model.SorFileId == traceStateVm.SorFileId));
            }

            var mustBeOpen = isUserAskedToOpenView || isTraceStateChanged;
            if (vm == null && !mustBeOpen)
                return;

            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(vm);
            }

            vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(traceStateVm, isLastMeasurementOnThisTrace, isTraceStateChanged);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(vm);
        }
    }
}