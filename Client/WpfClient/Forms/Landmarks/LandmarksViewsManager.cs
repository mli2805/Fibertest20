using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LandmarksViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly ChildrenViews _childrenViews;
        private readonly Model _readModel;
        private readonly TraceChoiceViewModel _traceChoiceViewModel;
        private List<LandmarksViewModel> LaunchedViews { get; } = new List<LandmarksViewModel>();

        public LandmarksViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            ChildrenViews childrenViews, Model readModel, TraceChoiceViewModel traceChoiceViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _childrenViews = childrenViews;
            _readModel = readModel;
            _traceChoiceViewModel = traceChoiceViewModel;

            childrenViews.PropertyChanged += ChildrenViews_PropertyChanged;
        }

        private void ChildrenViews_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews) sender).ShouldBeClosed)
                {
                    foreach (var traceStateViewModel in LaunchedViews.ToArray())
                    {
                        traceStateViewModel.TryClose();
                        LaunchedViews.Remove(traceStateViewModel);
                    }
                }
            }
        }

        public async Task InitializeFromRtu(Guid rtuId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            await vm.InitializeFromRtu(rtuId);
            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async Task InitializeFromTrace(Guid traceId, Guid selectedNodeId)
        {
            var vm = _globalScope.Resolve<LandmarksViewModel>();
            await vm.InitializeFromTrace(traceId, selectedNodeId);
            LaunchedViews.Add(vm);
            _childrenViews.ShouldBeClosed = false;
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async Task InitializeFromNode(Guid nodeId)
        {
            var traces = _readModel.Traces.Where(t => t.NodeIds.Contains(nodeId)).ToList();
            if (traces.Count == 0) return;
            if (traces.Count == 1)
            {
                await InitializeFromTrace(traces.First().TraceId, nodeId);
                return;
            }

            _traceChoiceViewModel.Initialize(traces);
            _windowManager.ShowDialogWithAssignedOwner(_traceChoiceViewModel);
            if (!_traceChoiceViewModel.IsAnswerPositive)
                return;
            var traceId = _traceChoiceViewModel.SelectedTrace.TraceId;
            await InitializeFromTrace(traceId, nodeId);
        }

        public async Task Apply(object e)
        {
            switch (e)
            {
                case RtuUpdated _: 
                case EquipmentUpdated _:
                case EquipmentIntoTraceIncluded _:
                case EquipmentFromTraceExcluded _:
                case NodeUpdatedAndMoved _:
                case NodeUpdated _: 
                case NodeMoved _: 
                    foreach (var v in LaunchedViews) 
                        await v.RefreshOnChangedTrace(); 
                    return;
            }

        }

    }
}
