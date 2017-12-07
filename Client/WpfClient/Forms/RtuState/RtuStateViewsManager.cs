using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewsManager
    {
        private readonly IMyWindowManager _windowManager;
        private readonly RtuStateVmFactory _rtuStateVmFactory;
        private Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(IMyWindowManager windowManager, RtuStateVmFactory rtuStateVmFactory)
        {
            _windowManager = windowManager;
            _rtuStateVmFactory = rtuStateVmFactory;
        }

        public void ShowRtuState(RtuLeaf rtuLeaf)
        {
            var rtuId = rtuLeaf.Id;

            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(rtuId, out vm))
            {
                vm.Close();
                LaunchedViews.Remove(rtuId);
            }

            vm = new RtuStateViewModel();
            vm.Initialize(_rtuStateVmFactory.Create(rtuLeaf));
            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(rtuId, vm);
        }

        public void NotifyUserRtuAvailabilityChanged(RtuLeaf rtuLeaf)
        {
            ShowRtuState(rtuLeaf);
        }

        public void NotifyUserRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(dto.RtuId, out vm))
                vm.NotifyUserCurrentMonitoringStep(dto);
        }

        public void NotifyUserMonitoringResult(Measurement dto)
        {
            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(dto.RtuId, out vm))
                vm.NotifyUserMonitoringResult(dto);
        }

        public void CleanClosedView()
        {

        }
    }
}