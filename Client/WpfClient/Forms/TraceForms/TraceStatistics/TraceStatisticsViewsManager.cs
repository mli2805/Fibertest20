using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewsManager
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private Dictionary<Guid, TraceStatisticsViewModel> LaunchedViews { get; } = new Dictionary<Guid, TraceStatisticsViewModel>();

        public TraceStatisticsViewsManager(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt: AddMeasurement(evnt); return;
                case MeasurementUpdated evnt: UpdateMeasurement(evnt); return;
                case TraceUpdated evnt: UpdateTrace(evnt); return;
                case ResponsibilitiesChanged evnt: ChangeResponsibility(evnt); return;
            }
        }

        private void ClearClosedViews()
        {
            var closed = (from pair in LaunchedViews where !pair.Value.IsOpen select pair.Key).ToList();
            foreach (var view in closed)
            {
                LaunchedViews.Remove(view);
            }
        }

        public void Show(Guid traceId)
        {
            ClearClosedViews();
            if (LaunchedViews.TryGetValue(traceId, out var vm))
            {
                vm.TryClose();
                LaunchedViews.Remove(traceId);
            }

            vm = _globalScope.Resolve<TraceStatisticsViewModel>();
            vm.Initialize(traceId);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(traceId, vm);
        }

        private void AddMeasurement(MeasurementAdded evnt)
        {
            ClearClosedViews();
            var traceId = evnt.TraceId;

            if (LaunchedViews.TryGetValue(traceId, out var vm))
                vm.AddNewMeasurement();
        }

        private void UpdateMeasurement(MeasurementUpdated evnt)
        {

        }

        private void UpdateTrace(TraceUpdated evnt)
        {

        }

        private void ChangeResponsibility(ResponsibilitiesChanged evnt)
        {

        }

    }
}