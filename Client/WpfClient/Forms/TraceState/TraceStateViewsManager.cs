using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewsManager
    {
        private readonly TraceStateModelFactory _traceStateModelFactory;
        private readonly IWindowManager _windowManager;
        private readonly C2DWcfManager _c2DWcfManager;

        private List<TraceStateViewModel> LaunchedViews { get; set; } = new List<TraceStateViewModel>();


        public TraceStateViewsManager(TraceStateModelFactory traceStateModelFactory,
            IWindowManager windowManager, C2DWcfManager c2DWcfManager)
        {
            _traceStateModelFactory = traceStateModelFactory;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        // User clicked on TraceLeaf - State
        public async void ShowTraceState(Guid traceId)
        {
            MeasurementWithSor measurementWithSor =  await _c2DWcfManager.GetLastMeasurementForTrace(traceId);
            var traceStateModel = _traceStateModelFactory.CreateModel(measurementWithSor.Measurement, measurementWithSor.SorData);
            Show(traceStateModel, true);
        }

        // MonitoringResult arrived by WCF
        public void NotifyAboutMonitoringResult(MeasurementWithSor measurementWithSor)
        {
            var traceStateModel = _traceStateModelFactory.CreateModel(measurementWithSor.Measurement, measurementWithSor.SorData);
            Show(traceStateModel, 
                isLastMeasurementOnThisTrace: true, 
                isUserAskedToOpenView: false, 
                isTraceStateChanged: measurementWithSor.Measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent);
        }

        // User clicked on line in TraceStatistics (maybe not on the last line - see parameter)
        public async void ShowTraceState(Measurement measurement, bool isLastMeasurementOnThisTrace)
        {
            var sorBytes = await _c2DWcfManager.GetSorBytes(measurement.SorFileId);
            if (sorBytes == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(measurement, sorBytes);
            Show(traceStateModel, isLastMeasurementOnThisTrace);
        }

        // User clicked on line in OpticalEvents (maybe last or not last line, and last event could be not last measurement for this trace) 
        public async void ShowTraceState(OpticalEventModel opticalEventModel)
        {
            var sorBytes = await _c2DWcfManager.GetSorBytes(opticalEventModel.SorFileId);
            if (sorBytes == null)
                return;

            var traceStateModel = _traceStateModelFactory.CreateModel(opticalEventModel, sorBytes);
            var isLastMeasurementOnThisTrace =
                await IsOpticalEventIsLastMeasurementForTrace(opticalEventModel.TraceId, opticalEventModel.SorFileId);
            Show(traceStateModel, isLastMeasurementOnThisTrace);
        }

        private async Task<bool> IsOpticalEventIsLastMeasurementForTrace(Guid traceId, int opticalEventSorFileId)
        {
            var measurementWithSor = await _c2DWcfManager.GetLastMeasurementForTrace(traceId);
            return measurementWithSor.Measurement.SorFileId == opticalEventSorFileId;
        }

        private void Show(TraceStateModel traceStateModel, bool isLastMeasurementOnThisTrace, bool isUserAskedToOpenView = true, bool isTraceStateChanged = false)
        {
            LaunchedViews.RemoveAll(v => !v.IsOpen);

            TraceStateViewModel vm;
            if (isLastMeasurementOnThisTrace)
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateModel.TraceId &&
                                                  (v.Model.SorFileId == traceStateModel.SorFileId ||
                                                    v.IsLastStateForThisTrace));
            }
            else
            {
                vm = LaunchedViews.FirstOrDefault(v => v.Model.TraceId == traceStateModel.TraceId &&
                                                       (v.Model.SorFileId == traceStateModel.SorFileId));
            }

            var mustBeOpen = isUserAskedToOpenView || isTraceStateChanged;
            if (vm == null && !mustBeOpen)
                return;

            if (vm != null)
            {
                vm.TryClose();
                LaunchedViews.Remove(vm);
            }

            vm = IoC.Get<TraceStateViewModel>();
            vm.Initialize(traceStateModel, isLastMeasurementOnThisTrace, isTraceStateChanged);
            _windowManager.ShowWindowWithAssignedOwner(vm);

            LaunchedViews.Add(vm);
        }
    }
}