using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuBanchAutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _desktopC2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly FailedAutoBasePdfProvider _failedAutoBasePdfProvider;
        private readonly MonitoringSettingsModelFactory _monitoringSettingsModelFactory;
        private List<TraceLeaf> _traceLeaves;
        private int _currentTraceIndex;
        public bool IsOpen { get; set; }
        public BanchOfMeasurementsExecutor BanchOfMeasurementsExecutor { get; }
        public bool ShouldStartMonitoring { get; set; }
        private RtuLeaf _rtuLeaf;
        private Rtu _rtu;

        private List<MeasurementCompletedEventArgs> _badResults;
        private List<TraceLeaf> _goodTraceLeaves;

        public RtuBanchAutoBaseViewModel(ILifetimeScope globalScope, IMyLog logFile, Model readModel, IWindowManager windowManager,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            BanchOfMeasurementsExecutor banchOfMeasurementsExecutor, FailedAutoBasePdfProvider failedAutoBasePdfProvider,
            MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _failedAutoBasePdfProvider = failedAutoBasePdfProvider;
            _monitoringSettingsModelFactory = monitoringSettingsModelFactory;
            BanchOfMeasurementsExecutor = banchOfMeasurementsExecutor;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
            BanchOfMeasurementsExecutor.MeasurementCompleted += BanchOfMeasurementsExecutor_MeasurementCompleted;
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _goodTraceLeaves = new List<TraceLeaf>();
            _badResults = new List<MeasurementCompletedEventArgs>();
            _traceLeaves = rtuLeaf.GetAttachedTraces();
            if (_traceLeaves.Count == 0)
                return false;
            _currentTraceIndex = 0;

            _rtuLeaf = rtuLeaf;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);

            if (!BanchOfMeasurementsExecutor.Initialize(_rtu))
                return false;

            ShouldStartMonitoring = true;
            BanchOfMeasurementsExecutor.Model.IsEnabled = true;

            return true;
        }

        private async void BanchOfMeasurementsExecutor_MeasurementCompleted(object sender, EventArgs e)
        {
            var result = (MeasurementCompletedEventArgs)e;
            _logFile.AppendLine($@"Measurement on trace {_traceLeaves[_currentTraceIndex].Title}: {result.CompletedStatus}");
            BanchOfMeasurementsExecutor.Model.TraceResults
                .Add(string.Format(Resources.SID__0___1___Measurement_on_trace__2____3_, 
                    _currentTraceIndex + 1, 
                    _traceLeaves.Count, 
                    _traceLeaves[_currentTraceIndex].Title, 
                    result.CompletedStatus.GetLocalizedString()));

            if (result.CompletedStatus != MeasurementCompletedStatus.BaseRefAssignedSuccessfully)
            {
                result.TraceLeaf = _traceLeaves[_currentTraceIndex];
                _badResults.Add(result);
            }
            else _goodTraceLeaves.Add(_traceLeaves[_currentTraceIndex]);

            if (++_currentTraceIndex < _traceLeaves.Count)
            {
                Thread.Sleep(1000);
                StartOneMeasurement();
            }
            else
            {
                _waitCursor.Dispose(); 
                await ApplyResults();
                BanchOfMeasurementsExecutor.Model.IsEnabled = true;
                BanchOfMeasurementsExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                BanchOfMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Collapsed;
                BanchOfMeasurementsExecutor.Model.TraceResults.Clear();
                TryClose();
            }
        }

        private WaitCursor _waitCursor;
        public async void Start()
        {
            _waitCursor = new WaitCursor();
            BanchOfMeasurementsExecutor.Model.IsEnabled = false;
            BanchOfMeasurementsExecutor.Model.TraceResultsVisibility = Visibility.Visible;
            BanchOfMeasurementsExecutor.Model.TraceResults
                .Add(string.Format(Resources.SID_There_are__0__trace_s__attached_to_ports_of_the_RTU, _traceLeaves.Count));

            // !!!!!!!!!!!!!!!!!!!!!!!!!!
            // var dto = _rtuLeaf
            //     .CreateDoClientMeasurementDto(_readModel, BanchOfMeasurementsExecutor.Model.CurrentUser)
            //     .SetParams(true, true, BanchOfMeasurementsExecutor.Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
            //         BanchOfMeasurementsExecutor.Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());
            // var startResult = await _commonC2DWcfManager.DoClientMeasurementAsync(dto);
            // _logFile.AppendLine(startResult.ReturnCode.GetLocalizedString());

            await BanchOfMeasurementsExecutor.Start(_rtuLeaf);
        }

        private async void StartOneMeasurement()
        {
            await BanchOfMeasurementsExecutor.Start(_rtuLeaf);
        }


        private async Task ApplyResults()
        {
            if (_badResults.Any())
                ShowReport();
            if (ShouldStartMonitoring && _goodTraceLeaves.Any())
                await StartMonitoring();
        }

        private async Task StartMonitoring()
        {
            BanchOfMeasurementsExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            BanchOfMeasurementsExecutor.Model.MeasurementProgressViewModel.Message1 = Resources.SID_Starting_monitoring;
            BanchOfMeasurementsExecutor.Model.MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;
            var monitoringSettingsModel = _monitoringSettingsModelFactory.Create(_rtuLeaf, false);

            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = monitoringSettingsModel.CreateDto();
                dto.Ports = _goodTraceLeaves
                    .Select(goodTraceLeaf => _readModel.Traces.First(t => t.TraceId == goodTraceLeaf.Id))
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
            callback(true);
        }
    }
}
