using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly TraceStateVmFactory _traceStateVmFactory;
        private readonly IMyWindowManager _windowManager;

        private List<TraceStateViewModel> LaunchedViews { get; set; } = new List<TraceStateViewModel>();


        public TraceStateViewsManager(TraceStateVmFactory traceStateVmFactory,
            IMyWindowManager windowManager)
        {
            _traceStateVmFactory = traceStateVmFactory;
            _windowManager = windowManager;
        }

        // User clicked on TraceLeaf - State
        public async void ShowTraceState(Guid traceId)
        {
            var traceStateVm = await _traceStateVmFactory.Create(traceId);
            Show(traceStateVm, true, true);
        }

        // MonitoringResult arrived by Wcf
        public void NotifyAboutMonitoringResult(Measurement measurement)
        {
            var mustBeOpen = (measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent);

            var traceStateVm = _traceStateVmFactory.CreateVm(measurement);
            Show(traceStateVm, true, mustBeOpen);
        }

        // User clicked on line in TraceStatistics (maybe not on the last line - see parameter)
        public void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace)
        {
            var traceStateVm = _traceStateVmFactory.CreateVm(measurement);
            Show(traceStateVm, isLastMeasurementOnThisTrace, true);
        }

        // User clicked on line in OpticalEvents (maybe last or not last line, and last event could be not last measurement for this trace) 
        public void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            var traceStateVm = _traceStateVmFactory.CreateVm(opticalEventVm);
            var temp = true;

            Show(traceStateVm, temp, true);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="traceStateVm"></param>
        /// <param name="isLastMeasurementOnThisTrace"></param>
        /// <param name="mustBeOpen"> 
        /// false - it is monitoring result arrival (with no state changed) 
        /// and new view shouldnt be shown open, 
        /// but if for this trace last state view opened already - change timestamp
        /// true - trace state changed - view should be open 
        /// </param>
        private void Show(TraceStateVm traceStateVm, bool isLastMeasurementOnThisTrace, bool mustBeOpen)
        {
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

            if (vm == null && !mustBeOpen)
                return;

            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(vm);
            }

            vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(traceStateVm, isLastMeasurementOnThisTrace);
            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(vm);
        }
    }
}