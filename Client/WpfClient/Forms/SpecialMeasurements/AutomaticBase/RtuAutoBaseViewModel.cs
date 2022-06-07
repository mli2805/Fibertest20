using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
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
        private readonly FailedAutoBasePdfProvider _failedAutoBasePdfProvider;
        private List<TraceLeaf> _traceLeaves;
        private int _currentTraceIndex;
        public bool IsOpen { get; set; }
        public OneMeasurementExecutor OneMeasurementExecutor { get; }
        public bool ShouldStartMonitoring { get; set; }
        private Rtu _rtu;
        private List<MeasurementCompletedEventArgs> _pdfSource;

        public RtuAutoBaseViewModel(IMyLog logFile, Model readModel, IWindowManager windowManager,
            OneMeasurementExecutor oneMeasurementExecutor, FailedAutoBasePdfProvider failedAutoBasePdfProvider)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _failedAutoBasePdfProvider = failedAutoBasePdfProvider;
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
            _pdfSource = new List<MeasurementCompletedEventArgs>();
            _traceLeaves = rtuLeaf.GetAttachedTraces();
            if (_traceLeaves.Count == 0)
                return false;
            _currentTraceIndex = 0;

            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            if (!OneMeasurementExecutor.Initialize(_rtu, true))
                return false;

            ShouldStartMonitoring = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

            return true;
        }

        private void OneMeasurementExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementCompletedEventArgs)e;
            _logFile.AppendLine($@"Measurement on trace {_traceLeaves[_currentTraceIndex].Title}: {result.CompletedStatus}");
            if (result.CompletedStatus != MeasurementCompletedStatus.BaseRefAssignedSuccessfully)
            {
                result.TraceLeaf = _traceLeaves[_currentTraceIndex];
                _pdfSource.Add(result);
            }
        
            if (++_currentTraceIndex < _traceLeaves.Count)
            {
                Thread.Sleep(1000);
                StartOneMeasurement();
            }
            else
            {
                _waitCursor.Dispose();
                ShowReport();
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

        private void ShowReport()
        {
            var report = _failedAutoBasePdfProvider.Create(_rtu, _pdfSource);
            PdfExposer.Show(report, 
                $@"FailedAutoBaseMeasurementsReport{DateTime.Now:yyyyMMddHHmmss}.pdf", 
                _windowManager);
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
