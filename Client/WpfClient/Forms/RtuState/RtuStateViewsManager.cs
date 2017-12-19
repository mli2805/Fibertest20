using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewsManager
    {
        private readonly IWindowManager _windowManager;
        private readonly RtuStateModelFactory _rtuStateModelFactory;
        private Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(IWindowManager windowManager, RtuStateModelFactory rtuStateModelFactory)
        {
            _windowManager = windowManager;
            _rtuStateModelFactory = rtuStateModelFactory;
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
            vm.Initialize(_rtuStateModelFactory.Create(rtuLeaf));
            _windowManager.ShowWindowWithAssignedOwner(vm);
//            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(rtuId, vm);
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
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