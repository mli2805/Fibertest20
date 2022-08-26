using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseProgress
    {
        public readonly TraceLeaf TraceLeaf;
        public readonly Trace Trace;
        public readonly int Ordinal;
        public bool MeasurementDone;
        public bool BaseRefAssigned;

        public RtuAutoBaseProgress(TraceLeaf traceLeaf, Trace trace, int ordinal)
        {
            TraceLeaf = traceLeaf;
            Trace = trace;
            Ordinal = ordinal;
        }
    }
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _desktopC2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly FailedAutoBasePdfProvider _failedAutoBasePdfProvider;
        private readonly MonitoringSettingsModelFactory _monitoringSettingsModelFactory;
        private List<RtuAutoBaseProgress> _progress;
        public bool IsOpen { get; set; }
        public WholeRtuMeasurementsExecutor WholeRtuMeasurementsExecutor { get; }
        public bool ShouldStartMonitoring { get; set; }
        private RtuLeaf _rtuLeaf;
        private Rtu _rtu;

        private List<MeasurementCompletedEventArgs> _badResults;
        private List<Trace> _goodTraces;

        public RtuAutoBaseViewModel(ILifetimeScope globalScope, IMyLog logFile,
            IDispatcherProvider dispatcherProvider, Model readModel, IWindowManager windowManager,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            WholeRtuMeasurementsExecutor wholeRtuMeasurementsExecutor, FailedAutoBasePdfProvider failedAutoBasePdfProvider,
            MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _dispatcherProvider = dispatcherProvider;
            _readModel = readModel;
            _windowManager = windowManager;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _failedAutoBasePdfProvider = failedAutoBasePdfProvider;
            _monitoringSettingsModelFactory = monitoringSettingsModelFactory;
            WholeRtuMeasurementsExecutor = wholeRtuMeasurementsExecutor;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            WholeRtuMeasurementsExecutor.MeasurementCompleted += OneMeasurementExecutor_MeasurementCompleted;
            WholeRtuMeasurementsExecutor.BaseRefAssigned += OneMeasurementExecutor_BaseRefAssigned;
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _goodTraces = new List<Trace>();
            _badResults = new List<MeasurementCompletedEventArgs>();
            var i = 0;
            _progress = rtuLeaf
                .GetAttachedTraces()
                .Select(t => new RtuAutoBaseProgress(
                    t, _readModel.Traces.First(tt => tt.TraceId == t.Id), ++i))
                .ToList();
            if (_progress.Count == 0)
                return false;

            _rtuLeaf = rtuLeaf;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            if (!WholeRtuMeasurementsExecutor.Initialize(_rtu, true))
                return false;

            // ShouldStartMonitoring = true;
            WholeRtuMeasurementsExecutor.Model.IsEnabled = true;

            return true;
        }

        private WaitCursor _waitCursor;
        public void Start()
        {
            _waitCursor = new WaitCursor();
            WholeRtuMeasurementsExecutor.Model.IsEnabled = false;
            WholeRtuMeasurementsExecutor.Model.TraceResults.Clear();
            WholeRtuMeasurementsExecutor.Model.TraceResults
                .Add(string.Format(Resources.SID_There_are__0__trace_s__attached_to_ports_of_the_RTU, _progress.Count));
            WholeRtuMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Visible;
            _logFile.EmptyLine();
            var progressItem = _progress.First();
            Task.Factory.StartNew(() =>
                WholeRtuMeasurementsExecutor.Start(progressItem, progressItem.Ordinal < _progress.Count));
        }

        private async void OneMeasurementExecutor_MeasurementCompleted(object sender, MeasurementCompletedEventArgs result)
        {
            var progressItem = _progress.First(i => i.Trace.TraceId == result.Trace.TraceId);
            progressItem.MeasurementDone = true;
            _logFile.AppendLine($@"Measurement on trace {result.Trace.Title}: {result.CompletedStatus}");

            var line = $@"{progressItem.Ordinal}/{_progress.Count} {result.Trace.Title} : {result.CompletedStatus.KhazanovStyle()}";

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                WholeRtuMeasurementsExecutor.Model.TraceResults.Add(line);
            }); // sync, GUI thread

            if (result.CompletedStatus != MeasurementCompletedStatus.MeasurementCompletedSuccessfully)
            {
                _badResults.Add(result);
                progressItem.BaseRefAssigned = true; // nothing to assign
            }
            else
            {
                _logFile.AppendLine($@"Assign base refs for {result.Trace.Title}", 2);
                await Task.Factory.StartNew(() => WholeRtuMeasurementsExecutor.SetAsBaseRef(result.SorBytes, result.Trace));
            }

            var nextItem = _progress.FirstOrDefault(i => !i.MeasurementDone);
            if (nextItem != null)
            {
                _logFile.AppendLine($@"Start next measurement for {nextItem.Trace.Title}");
                Thread.Sleep(100);
                await Task.Factory.StartNew(() =>
                    WholeRtuMeasurementsExecutor.Start(nextItem, nextItem.Ordinal < _progress.Count));
            }
            else
            {
                if (_progress.All(i => i.MeasurementDone && i.BaseRefAssigned))
                {
                    await _dispatcherProvider.GetDispatcher().Invoke(Finish);
                }
            }
        }

        private void OneMeasurementExecutor_BaseRefAssigned(object sender, MeasurementCompletedEventArgs result)
        {
            _dispatcherProvider.GetDispatcher().Invoke(() => ProcessBaseRefAssignedResult(result)); // sync, GUI thread
        }

        private async void ProcessBaseRefAssignedResult(MeasurementCompletedEventArgs result)
        {
            var progressItem = _progress.First(i => i.Trace.TraceId == result.Trace.TraceId);
            progressItem.BaseRefAssigned = true;
            _logFile.AppendLine($@"Assigned base ref for trace {result.Trace.Title}: {result.CompletedStatus}", 2);

            var line = $@"{progressItem.Ordinal}/{_progress.Count} {result.Trace.Title} : {result.CompletedStatus.KhazanovStyle()}";

            WholeRtuMeasurementsExecutor.Model.TraceResults.Add(line);

            if (result.CompletedStatus != MeasurementCompletedStatus.BaseRefAssignedSuccessfully)
                _badResults.Add(result);
            else
                _goodTraces.Add(result.Trace);

            if (_progress.All(i => i.MeasurementDone && i.BaseRefAssigned))
            {
                await _dispatcherProvider.GetDispatcher().Invoke(Finish);
            }
        }

        private async Task Finish()
        {
            _waitCursor.Dispose();
            _logFile.EmptyLine();

            if (_badResults.Any())
                ShowReport();
            if (ShouldStartMonitoring && _goodTraces.Any())
                await StartMonitoring();

            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;

            var timestamp = $@"{DateTime.Now:d} {DateTime.Now:t}";
            var strs = new List<string>() { Resources.SID_The_process_of_setting_base_ref_for_RTU, _rtu.Title, Resources.SID_is_completed_at_ + timestamp };
            var mb = new MyMessageBoxViewModel(MessageType.Information, strs);
            _windowManager.ShowDialogWithAssignedOwner(mb);

            WholeRtuMeasurementsExecutor.Model.IsEnabled = true;
            WholeRtuMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Collapsed;
            WholeRtuMeasurementsExecutor.Model.TraceResults.Clear();
            TryClose();
        }

        private async Task StartMonitoring()
        {
            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.Message1 = Resources.SID_Starting_monitoring;
            WholeRtuMeasurementsExecutor.Model.MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;
            var monitoringSettingsModel = _monitoringSettingsModelFactory.Create(_rtuLeaf, false);

            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = monitoringSettingsModel.CreateDto();
                dto.Ports = _goodTraces
                    .Select(trace => new PortWithTraceDto()
                    {
                        OtauPort = trace.OtauPort,
                        TraceId = trace.TraceId
                    }).ToList();
                dto.IsMonitoringOn = true;

                var resultDto = await _commonC2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = dto.CreateCommand();
                    var result = await _desktopC2DWcfManager.SendCommandAsObj(cmd);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var mb = new MyMessageBoxViewModel(MessageType.Error, result);
                        _windowManager.ShowDialogWithAssignedOwner(mb);
                    }
                }
            }
        }

        private void ShowReport()
        {
            var report = _failedAutoBasePdfProvider.Create(_rtu, _badResults);
            PdfExposer.Show(report, $@"FailedAutoBaseMeasurementsReport{DateTime.Now:yyyyMMddHHmmss}.pdf",
                _windowManager);
        }

        public void Close()
        {
            TryClose();
        }
        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            WholeRtuMeasurementsExecutor.MeasurementCompleted -= OneMeasurementExecutor_MeasurementCompleted;
            WholeRtuMeasurementsExecutor.BaseRefAssigned -= OneMeasurementExecutor_BaseRefAssigned;
            callback(true);
        }
    }
}
