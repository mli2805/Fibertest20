﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class OneMeasurementExecutor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly MeasurementDtoProvider _measurementDtoProvider;
        private readonly VeexMeasurementFetcher _veexMeasurementFetcher;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace;
        public OneMeasurementModel Model { get; set; } = new OneMeasurementModel();

        public OneMeasurementExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            MeasurementDtoProvider measurementDtoProvider, VeexMeasurementFetcher veexMeasurementFetcher,
            MeasurementAsBaseAssigner measurementAsBaseAssigner
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _dispatcherProvider = dispatcherProvider;
            _measurementDtoProvider = measurementDtoProvider;
            _veexMeasurementFetcher = veexMeasurementFetcher;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
        }

        public bool Initialize(Rtu rtu, bool isForRtu)
        {
            Model.Rtu = rtu;

            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, isForRtu);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        public async Task Start(TraceLeaf traceLeaf)
        {
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(traceLeaf.Title);

            var dto = _measurementDtoProvider
                .Initialize(traceLeaf, true)
                .PrepareDto(Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                _timer.Stop();
                _timer.Dispose();
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;
                Model.IsEnabled = true;

                var list = new List<string>() { startResult.ReturnCode.GetLocalizedString(), startResult.ErrorMessage };
                MeasurementCompleted?
                    .Invoke(this, new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToStart, list));

                Model.IsEnabled = true;
                return;
            }

            Model.MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (Model.Rtu.RtuMaker == RtuMaker.VeEX)
            {
                var veexResult = await _veexMeasurementFetcher.Fetch(dto.RtuId, startResult.ClientMeasurementId);
                if (veexResult.CompletedStatus == MeasurementCompletedStatus.MeasurementCompletedSuccessfully)
                    ProcessMeasurementResult(veexResult.SorBytes);
                else
                {
                    _timer.Stop();
                    _timer.Dispose();
                    MeasurementCompleted?.Invoke(this, veexResult);
                }
            } // if RtuMaker is IIT - result will come through WCF contract

        }

        private System.Timers.Timer _timer;
        private void StartTimer()
        {
            _logFile.AppendLine(@"Start a measurement timeout");
            _timer = new System.Timers.Timer(Model.MeasurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();
        }
        private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logFile.AppendLine(@"Measurement timeout expired");
            _timer.Dispose();

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;

                MeasurementCompleted?
                    .Invoke(this,
                        new MeasurementCompletedEventArgs(
                            MeasurementCompletedStatus.MeasurementTimeoutExpired,
                            new List<string>()
                            {
                                Resources.SID_Base_reference_assignment_failed, "",
                                Resources.SID_Measurement_timeout_expired
                            }));
                Model.IsEnabled = true;
            });
        }

        public async void ProcessMeasurementResult(byte[] sorBytes)
        {
            _timer.Stop();
            _timer.Dispose();

            _logFile.AppendLine(@"Measurement (Client) result received");

            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var sorData = SorData.FromBytes(sorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner
                .Assign(sorData, _trace, Model.MeasurementProgressViewModel);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementCompletedEventArgs(MeasurementCompletedStatus.BaseRefAssignedSuccessfully, "", sorBytes)
                    : new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToAssignAsBase, result.ErrorMessage));
        }

        public delegate void MeasurementHandler(object sender, MeasurementCompletedEventArgs e);

        public event MeasurementHandler MeasurementCompleted;
    }
}