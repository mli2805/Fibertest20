﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RtuBatchAutoBaseViewModel : Screen
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
        public OneMeasurementExecutor OneMeasurementExecutor { get; }
        public bool ShouldStartMonitoring { get; set; }
        private RtuLeaf _rtuLeaf;
        private Rtu _rtu;

        private List<MeasurementCompletedEventArgs> _badResults;
        private List<TraceLeaf> _goodTraceLeaves;

        public RtuBatchAutoBaseViewModel(ILifetimeScope globalScope, IMyLog logFile, Model readModel, IWindowManager windowManager,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            OneMeasurementExecutor oneMeasurementExecutor, FailedAutoBasePdfProvider failedAutoBasePdfProvider,
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
            OneMeasurementExecutor = oneMeasurementExecutor;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
            IsOpen = true;
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

            ShouldStartMonitoring = true;
            OneMeasurementExecutor.Model.IsEnabled = true;

            return true;
        }

        private WaitCursor _waitCursor;
        public void Start()
        {
            _waitCursor = new WaitCursor();
            OneMeasurementExecutor.Model.IsEnabled = false;
            OneMeasurementExecutor.Model.TraceResultsVisibility = Visibility.Visible;
            OneMeasurementExecutor.Model.TraceResults
                .Add(string.Format(Resources.SID_There_are__0__trace_s__attached_to_ports_of_the_RTU, _traceLeaves.Count));

            // !!!!!!!!!!!!!!!!!!!!!!!!!!

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
            OneMeasurementExecutor.Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            OneMeasurementExecutor.Model.MeasurementProgressViewModel.Message1 = Resources.SID_Starting_monitoring;
            OneMeasurementExecutor.Model.MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;
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
