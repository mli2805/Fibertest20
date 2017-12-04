using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewsManager
    {
        private readonly IMyWindowManager _windowManager;
        private Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(IMyWindowManager windowManager)
        {
            _windowManager = windowManager;
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
            vm.Initialize(Prepare(rtuLeaf));
            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(rtuId, vm);
        }

        private RtuStateVm Prepare(RtuLeaf rtuLeaf)
        {
            var rtuStateVm = new RtuStateVm();
            rtuStateVm.Title = rtuLeaf.Title;

            rtuStateVm.MainAddressState = rtuLeaf.MainChannelState;
            rtuStateVm.ReserveAddressState = rtuLeaf.ReserveChannelState;

            rtuStateVm.FullPortCount = rtuLeaf.FullPortCount;
            rtuStateVm.OwnPortCount = rtuLeaf.OwnPortCount;

            rtuStateVm.BopState = rtuLeaf.BopState.ToLocalizedString();

            rtuStateVm.TracesState = FiberState.FiberBreak.ToLocalizedString();
            rtuStateVm.MonitoringMode = rtuLeaf.MonitoringState.ToLocalizedString();

            rtuStateVm.Ports = new List<PortLineVm>();
            return rtuStateVm;
        }

        public void NotifyUserRtuAvailabilityChanged(RtuLeaf rtuLeaf)
        {
            ShowRtuState(rtuLeaf);
        }

        public void CleanClosedView()
        {
            
        }
    }
}