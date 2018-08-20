using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class BopStateViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly ChildrenViews _childrenViews;

        public Dictionary<Guid, BopStateViewModel> LaunchedViews { get; set; } =
            new Dictionary<Guid, BopStateViewModel>();


        public BopStateViewsManager(ILifetimeScope globalScope, IWindowManager windowManager, 
            Model readModel, CurrentUser currentUser, ChildrenViews childrenViews)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _childrenViews = childrenViews;

            childrenViews.PropertyChanged += ChildrenViews_PropertyChanged;
        }

        private void ChildrenViews_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ChildrenViews.ShouldBeClosed))
            {
                if (((ChildrenViews) sender).ShouldBeClosed)
                {
                    foreach (var pair in LaunchedViews.ToList())
                    {
                        pair.Value.TryClose();
                        LaunchedViews.Remove(pair.Key);
                    }
                }
            }
        }

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case BopNetworkEventAdded e: NotifyUserBopStateChanged(e); return;
                case ResponsibilitiesChanged _: ChangeResponsibilities(); return;
                default: return;
            }
        }

        private void NotifyUserBopStateChanged(BopNetworkEventAdded bopNetworkEventAdded)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == bopNetworkEventAdded.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId)) return;

            Show(bopNetworkEventAdded);
        }

        private void ChangeResponsibilities()
        {
            foreach (var pair in LaunchedViews)
            {
                var rtu = _readModel.Rtus.First(r => r.Id == pair.Key);
                if (!rtu.ZoneIds.Contains(_currentUser.ZoneId))
                    pair.Value.TryClose();
            }
        }

        private void Show(BopNetworkEventAdded bopNetworkEventAdded)
        {
            ClearClosedViews();

            var vm = LaunchedViews.FirstOrDefault(m => m.Value.BopIp == bopNetworkEventAdded.OtauIp).Value;
            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(bopNetworkEventAdded.RtuId);
            }

            vm = _globalScope.Resolve<BopStateViewModel>();
            vm.Initialize(bopNetworkEventAdded);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(bopNetworkEventAdded.RtuId, vm);
            _childrenViews.ShouldBeClosed = false;
        }
        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }}
}
