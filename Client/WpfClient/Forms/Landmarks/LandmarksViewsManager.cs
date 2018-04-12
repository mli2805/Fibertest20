using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LandmarksViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private List<LandmarksViewModel> LaunchedViews { get; } = new List<LandmarksViewModel>();

        public LandmarksViewsManager(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public async Task<int> InitializeFromRtu(Guid rtuId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromRtu(rtuId);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromTrace(Guid traceId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromTrace(traceId);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromNode(Guid nodeId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromNode(nodeId);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case NodeUpdatedAndMoved evnt: UpdateAndMoveNode(evnt); return;
                case EquipmentUpdated evnt: UpdateEquipment(evnt); return;
            }
        }

        private void UpdateAndMoveNode(NodeUpdatedAndMoved evnt)
        {
            LaunchedViews.ForEach(v=>v.UpdateAndMoveNode(evnt));
        }

        private void UpdateEquipment(EquipmentUpdated evnt)
        {
            LaunchedViews.ForEach(v => v.UpdateEquipment(evnt));
        }
    }
}
