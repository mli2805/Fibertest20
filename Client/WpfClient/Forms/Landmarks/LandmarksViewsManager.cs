﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Model _readModel;
        private readonly TraceChoiceViewModel _traceChoiceViewModel;
        private List<LandmarksViewModel> LaunchedViews { get; } = new List<LandmarksViewModel>();

        public LandmarksViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            Model readModel, TraceChoiceViewModel traceChoiceViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _readModel = readModel;
            _traceChoiceViewModel = traceChoiceViewModel;
        }

        public async Task<int> InitializeFromRtu(Guid rtuId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromRtu(rtuId);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromTrace(Guid traceId, Guid selectedNodeId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            var res = await vm.InitializeFromTrace(traceId, selectedNodeId);
            LaunchedViews.Add(vm);
            _windowManager.ShowWindowWithAssignedOwner(vm);
            return res;
        }

        public async Task<int> InitializeFromNode(Guid nodeId)
        {
            var traces = _readModel.Traces.Where(t => t.NodeIds.Contains(nodeId)).ToList();
            if (traces.Count == 0) return -1;
            if (traces.Count == 1)
                return await InitializeFromTrace(traces.First().TraceId, nodeId);

            _traceChoiceViewModel.Initialize(traces);
            _windowManager.ShowDialogWithAssignedOwner(_traceChoiceViewModel);
            if (!_traceChoiceViewModel.IsAnswerPositive)
                return -1;
            var traceId = _traceChoiceViewModel.SelectedTrace.TraceId;
            return await InitializeFromTrace(traceId, nodeId);

//            var vm = _globalScope.Resolve<LandmarksViewModel>();
//            var res = await vm.InitializeFromNode(nodeId);
//            if (vm.SelectedTrace == null) return -1;
//            LaunchedViews.Add(vm);
//            _windowManager.ShowWindowWithAssignedOwner(vm);
//            return res;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case RtuUpdated _: 
                case EquipmentUpdated _:
                case EquipmentIntoTraceIncluded _:
                case EquipmentFromTraceExcluded _:
                case NodeUpdatedAndMoved _:
                case NodeUpdated _: 
                case NodeMoved _: LaunchedViews.ForEach(v=>v.RefreshAsChangesReaction()); return;
            }
        }

    }
}
