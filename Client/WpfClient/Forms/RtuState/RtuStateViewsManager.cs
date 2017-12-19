using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewsManager
    {
        private readonly IWindowManager _windowManager;
        private readonly RtuStateVmFactory _rtuStateVmFactory;
        private Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(IWindowManager windowManager, RtuStateVmFactory rtuStateVmFactory)
        {
            _windowManager = windowManager;
            _rtuStateVmFactory = rtuStateVmFactory;
        }

        public void ShowRtuState(RtuLeaf rtuLeaf)
        {
            ClearClosedViews();

            var rtuId = rtuLeaf.Id;

            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(rtuId, out vm))
            {
                vm.Close();
                LaunchedViews.Remove(rtuId);
            }

            vm = IoC.Get<RtuStateViewModel>();
            vm.Initialize(_rtuStateVmFactory.Create(rtuLeaf));
            _windowManager.ShowWindowWithAssignedOwner(vm);
//            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(rtuId, vm);
        }

        private void ClearClosedViews()
        {
            foreach (var pair in LaunchedViews)
            {
                if (!pair.Value.IsOpen)
                    LaunchedViews.Remove(pair.Key);
            }
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