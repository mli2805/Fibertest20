using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewsManager
    {
        private readonly IMyWindowManager _windowManager;
        private Dictionary<Guid, TraceStatisticsViewModel> LaunchedViews { get; set; } = new Dictionary<Guid, TraceStatisticsViewModel>();

        public TraceStatisticsViewsManager(IMyWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Show(Guid traceId)
        {
            TraceStatisticsViewModel vm;
            if (LaunchedViews.TryGetValue(traceId, out vm))
            {
                vm.TryClose();
                LaunchedViews.Remove(traceId);
            }

            vm = IoC.Get<TraceStatisticsViewModel>();
            vm.Initialize(traceId);
            _windowManager.ShowWindow(vm);

            LaunchedViews.Add(traceId, vm);
        }

        public void AddNewMeasurement(Measurement measurement)
        {
            var traceId = measurement.TraceId;

            TraceStatisticsViewModel vm;
            if (LaunchedViews.TryGetValue(traceId, out vm))
            {
                vm.AddNewMeasurement(measurement);
            }
            else
            {
                vm = IoC.Get<TraceStatisticsViewModel>();
                vm.Initialize(traceId);
                _windowManager.ShowWindow(vm);

                LaunchedViews.Add(traceId, vm);
            }
        }

    }
}