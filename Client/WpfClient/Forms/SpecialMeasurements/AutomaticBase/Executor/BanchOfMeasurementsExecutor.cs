using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class BanchOfMeasurementsExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        public OneMeasurementModel Model { get; set; } = new OneMeasurementModel();

        public BanchOfMeasurementsExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            MeasurementAsBaseAssigner measurementAsBaseAssigner
        )
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
        }

        public bool Initialize(Rtu rtu)
        {
            Model.Rtu = rtu;

            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, true);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        public async Task Start(RtuLeaf rtuLeaf)
        {
            // StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(@"Let's get started!");

            var dto = rtuLeaf
                .CreateDoClientMeasurementDto(_readModel, Model.CurrentUser)
                .SetParams(true, Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                // _timer.Stop();
                // _timer.Dispose();
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

            // RtuMaker is IIT - result will come through WCF contract

        }
        //
        // private System.Timers.Timer _timer;
        // private void StartTimer()
        // {
        //     _logFile.AppendLine(@"Start a measurement timeout");
        //     _timer = new System.Timers.Timer(Model.MeasurementTimeout);
        //     _timer.Elapsed += TimeIsOver;
        //     _timer.AutoReset = false;
        //     _timer.Start();
        // }
        // private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        // {
        //     _logFile.AppendLine(@"Measurement timeout expired");
        //     // _timer.Dispose();
        //
        //     _dispatcherProvider.GetDispatcher().Invoke(() =>
        //     {
        //         Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
        //
        //         MeasurementCompleted?
        //             .Invoke(this,
        //                 new MeasurementCompletedEventArgs(
        //                     MeasurementCompletedStatus.MeasurementTimeoutExpired,
        //                     new List<string>()
        //                     {
        //                         Resources.SID_Base_reference_assignment_failed, "",
        //                         Resources.SID_Measurement_timeout_expired
        //                     }));
        //         Model.IsEnabled = true;
        //     });
        // }

        public async void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            // _timer.Stop();
            // _timer.Dispose();

            _logFile.AppendLine(@"Measurement (Client) result received");

            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var sorData = SorData.FromBytes(dto.SorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            if (!_readModel.TryGetTraceByOtauPortDto(dto.OtauPortDto, out Trace trace))
            {
                _logFile.AppendLine($@"Not found trace on otau {dto.OtauPortDto.OtauId} serial {dto.OtauPortDto.Serial} port {dto.OtauPortDto.OpticalPort}");
                return;
            }

            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner
                .Assign(sorData, trace, Model.MeasurementProgressViewModel);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementCompletedEventArgs(MeasurementCompletedStatus.BaseRefAssignedSuccessfully, "", dto.SorBytes)
                    : new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToAssignAsBase, result.ErrorMessage));
        }

        public delegate void MeasurementHandler(object sender, MeasurementCompletedEventArgs e);

        public event MeasurementHandler MeasurementCompleted;
    }
}