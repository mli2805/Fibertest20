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

        // from TraceLeaf
        public void ShowTraceState(Guid traceId)
        {
            var traceStateVm = _traceStateVmFactory.Create(traceId);
            Show(traceStateVm, true);
        }

        // from Accident happend
        public void NotifyAboutMonitoringResult(Measurement measurement)
        {
            var traceStateVm = _traceStateVmFactory.Create(measurement);
            Show(traceStateVm, true);
        }

        // from TraceStatistics
        public void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace)
        {
            var traceStateVm = _traceStateVmFactory.Create(measurement);

            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(traceStateVm, isLastMeasurementOnThisTrace);
            _windowManager.ShowWindow(vm);
        }

        // from OpticalEvents
        public void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            var traceStateVm = _traceStateVmFactory.Create(opticalEventVm);
            var temp = true;

            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(traceStateVm, temp);
            _windowManager.ShowWindow(vm);
        }


        //---------------------------------------------------------------
        private void Show(TraceStateVm traceStateVm, bool isLastMeasurementOnThisTrace)
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