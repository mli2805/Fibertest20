using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewsManager
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly RtuStateModelFactory _rtuStateModelFactory;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private Dictionary<Guid, RtuStateViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, RtuStateViewModel>();

        public RtuStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            RtuStateModelFactory rtuStateModelFactory, TreeOfRtuModel treeOfRtuModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _rtuStateModelFactory = rtuStateModelFactory;
            _treeOfRtuModel = treeOfRtuModel;
        }

        // user clicked on RtuLeaf
        public void ShowRtuState(RtuLeaf rtuLeaf)
        {
            Show(rtuLeaf, isUserAskedToOpenView: true, changes: RtuPartStateChanges.NoChanges);
        }

        // Server sent network event
        public void NotifyUserRtuAvailabilityChanged(NetworkEventAdded networkEventAdded)
        {
            var networkEvent = _mapper.Map<NetworkEvent>(networkEventAdded);
            RtuLeaf rtuLeaf = (RtuLeaf)_treeOfRtuModel.Tree.GetById(networkEvent.RtuId);
            Show(rtuLeaf, isUserAskedToOpenView: false, changes: networkEventAdded.RtuPartStateChanges);
        }

        public void NotifyUserRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            ClearClosedViews();
            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(dto.RtuId, out vm))
                vm.NotifyUserCurrentMonitoringStep(dto);
        }

        public void NotifyUserMonitoringResult(Measurement dto)
        {
            ClearClosedViews();
            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(dto.RtuId, out vm))
                vm.NotifyUserMonitoringResult(dto);
        }

        public void NotifyUserMonitoringStopped(Guid rtuId)
        {
            if (LaunchedViews.TryGetValue(rtuId, out var vm))
                vm.MonitoringStopped();
        }

        public void NotifyUserMonitoringStarted(Guid rtuId)
        {
            if (LaunchedViews.TryGetValue(rtuId, out var vm))
                vm.MonitoringStarted();
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }

        private void Show(RtuLeaf rtuLeaf, bool isUserAskedToOpenView, RtuPartStateChanges changes)
        {
            ClearClosedViews();

            var rtuId = rtuLeaf.Id;

            RtuStateViewModel vm;
            if (LaunchedViews.TryGetValue(rtuId, out vm))
            {
                vm.Close();
                LaunchedViews.Remove(rtuId);
            }

            
            vm = _globalScope.Resolve<RtuStateViewModel>();
            vm.Initialize(_rtuStateModelFactory.Create(rtuLeaf), isUserAskedToOpenView, changes);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(rtuId, vm);
        }



               


    }
}