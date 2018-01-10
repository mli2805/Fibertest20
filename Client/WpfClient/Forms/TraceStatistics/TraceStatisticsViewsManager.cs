using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

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

        public void AddNewMeasurement(Measurement measurement)
        {
            ClearClosedViews();
            var traceId = measurement.TraceId;

            TraceStatisticsViewModel vm;
            if (LaunchedViews.TryGetValue(traceId, out vm))
                vm.AddNewMeasurement(measurement);
        }

    }
}