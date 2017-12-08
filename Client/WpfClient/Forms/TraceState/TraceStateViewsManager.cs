using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly IMyLog _logFile;
        private readonly TraceStateVmFactory _traceStateVmFactory;
        private readonly ReadModel _readModel;
        private readonly IMyWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;

        private Dictionary<Guid, TraceStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, TraceStateViewModel>();


        public TraceStateViewsManager(IMyLog logFile, TraceStateVmFactory traceStateVmFactory, ReadModel readModel,
            IMyWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _traceStateVmFactory = traceStateVmFactory;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }



        // from TraceLeaf
        public void ShowTraceState(Guid traceId)
        {
            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(_traceStateVmFactory.Create(traceId));
            _windowManager.ShowWindow(vm);
        }

        // from TraceStatistics
        public void ShowTraceState(Guid traceId, MeasurementVm measurementVm)
        {
            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(_traceStateVmFactory.Create(traceId, measurementVm));
            _windowManager.ShowWindow(vm);
        }

        // from OpticalEvents
        public void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(_traceStateVmFactory.Create(opticalEventVm));
            _windowManager.ShowWindow(vm);
        }

        // from Accident happend
        public void NotifyAboutMonitoringResult(Measurement measurement)
        {
            var vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(_traceStateVmFactory.Create(measurement));
            _windowManager.ShowWindow(vm);
        }
    }
}