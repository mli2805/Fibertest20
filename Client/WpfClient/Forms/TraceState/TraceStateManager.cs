using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TraceStateManager
    {
        private readonly IWindowManager _windowManager;

        public TraceStateManager(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        // from TraceLeaf
        public void ShowTraceState(Guid traceId)
        {
            ShowTraceState(Prepare(traceId));

        }

        // from TraceStatistics
        public void ShowTraceState(MeasurementVm measurementVm)
        {
            ShowTraceState(Prepare(measurementVm));
        }

        // from OpticalEvents
        public void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            ShowTraceState(Prepare(opticalEventVm));
        }

        //----------------------------------------------------

        private void ShowTraceState(TraceStateVm traceStateVm)
        {
            var vm = new TraceStateViewModel();
            vm.Initialize(traceStateVm);
            _windowManager.ShowDialog(vm);
        }

        private TraceStateVm Prepare(Guid traceId)
        {
            return new TraceStateVm()
            {
//                BaseRefType = measurementVm.BaseRefType,
//                TraceState = measurementVm.TraceState,
            };
        }

        private TraceStateVm Prepare(MeasurementVm measurementVm)
        {
            return new TraceStateVm()
            {
                BaseRefType = measurementVm.BaseRefType,
                TraceState = measurementVm.TraceState,
            };
        }

        private TraceStateVm Prepare(OpticalEventVm opticalEventVm)
        {
            return new TraceStateVm()
            {
                TraceState = opticalEventVm.TraceState,
            };
        }




    }
}