using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private List<TraceLeaf> _traceLeaves;
        private int _currentTraceIndex;
        public bool IsOpen { get; set; }
        public OneMeasurementExecutor OneMeasurementExecutor { get; }
        public bool ShouldStartMonitoring { get; set; }

        public RtuAutoBaseViewModel(IMyLog logFile, Model readModel, IWindowManager windowManager, 
            OneMeasurementExecutor oneMeasurementExecutor)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            OneMeasurementExecutor = oneMeasurementExecutor;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            OneMeasurementExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _traceLeaves = rtuLeaf.GetAttachedTraces();
            if (_traceLeaves.Count == 0)
                return false;
            _currentTraceIndex = 0;

            var rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            if (!OneMeasurementExecutor.Initialize(rtu, true))
                return false;

            ShouldStartMonitoring = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementCompletedEventArgs)e;

            _logFile.AppendLine($@"Measurement on trace {result.Trace}: Model {result.ModelGuid.First6()};   Executor {result.ExecutorGuid.First6()};   {result.CompletedStatus}");
        
            // _currentTraceIndex++;
            if (++_currentTraceIndex < _traceLeaves.Count)
            {
                Thread.Sleep(1000);
                StartOneMeasurement();
            }
            else
            {
                _waitCursor.Dispose();
                OneMeasurementExecutor.Model.IsEnabled = true;
                TryClose();
            }
        }

        private WaitCursor _waitCursor;
        public void Start()
        {
            _waitCursor = new WaitCursor();
            OneMeasurementExecutor.Model.IsEnabled = false;
            StartOneMeasurement();
        }

        private async void StartOneMeasurement()
        {
            await OneMeasurementExecutor.Start(_traceLeaves[_currentTraceIndex]);
        }

        public void Close()
        {
            TryClose();
        }
        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            OneMeasurementExecutor.MeasurementCompleted -= OneMeasurementExecutor_MeasurementCompleted;
            callback(true);
        }
    }
}
