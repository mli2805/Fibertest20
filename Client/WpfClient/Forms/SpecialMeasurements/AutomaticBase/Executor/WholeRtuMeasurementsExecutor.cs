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
    public class WholeRtuMeasurementsExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly VeexMeasurementFetcher _veexMeasurementFetcher;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace;
        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public WholeRtuMeasurementsExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            VeexMeasurementFetcher veexMeasurementFetcher,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _dispatcherProvider = dispatcherProvider;
            _veexMeasurementFetcher = veexMeasurementFetcher;
            _landmarksIntoBaseSetter = landmarksIntoBaseSetter;
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

        public async Task Start(RtuAutoBaseProgress item, bool keepOtdrConnection = false)
        {
            _logFile.AppendLine($@"Start auto base measurement for {item.Trace.Title}");
            _trace = item.Trace;
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(item.Trace.Title);

            var dto = item.TraceLeaf.Parent
                .CreateDoClientMeasurementDto(item.TraceLeaf.PortNumber, keepOtdrConnection, _readModel, Model.CurrentUser)
                .SetParams(true, Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                _timer.Stop();
                _timer.Dispose();
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Hidden;
                Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;

                var list = new List<string>() { startResult.ReturnCode.GetLocalizedString(), startResult.ErrorMessage };
                MeasurementCompleted?
                    .Invoke(this, new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToStart, _trace, list));

                return;
            }

            Model.MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (Model.Rtu.RtuMaker == RtuMaker.VeEX)
            {
                var veexResult = await _veexMeasurementFetcher.Fetch(dto.RtuId, _trace, startResult.ClientMeasurementId);
                if (veexResult.CompletedStatus == MeasurementCompletedStatus.MeasurementCompletedSuccessfully)
                {
                    var res = new ClientMeasurementResultDto() { SorBytes = veexResult.SorBytes };
                    ProcessMeasurementResult(res);
                }
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
            _logFile.AppendLine($@"Start a measurement timeout for trace {_trace.Title}");
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
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Hidden;

                MeasurementCompleted?
                    .Invoke(this,
                        new MeasurementCompletedEventArgs(
                            MeasurementCompletedStatus.MeasurementTimeoutExpired,
                            _trace,
                            new List<string>()
                            {
                                Resources.SID_Base_reference_assignment_failed, "",
                                Resources.SID_Measurement_timeout_expired
                            }));
            });
        }

        public void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _timer.Stop();
            _timer.Dispose();

            _logFile.AppendLine($@"Measurement (Client) result for trace {_trace.Title} received");

            if (dto.SorBytes == null)
            {
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Hidden;
                MeasurementCompleted?
                    .Invoke(this, new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToStart,
                        _trace, dto.ReturnCode.GetLocalizedString()));
                return;
            }

            MeasurementCompleted?
                .Invoke(this, new MeasurementCompletedEventArgs(
                    MeasurementCompletedStatus.MeasurementCompletedSuccessfully, _trace, "", dto.SorBytes));
        }

        public async Task SetAsBaseRef(byte[] sorBytes, Trace trace)
        {
            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var sorData = SorData.FromBytes(sorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner
                .Assign(sorData, trace);

            BaseRefAssigned?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementCompletedEventArgs(MeasurementCompletedStatus.BaseRefAssignedSuccessfully, trace, "", sorData.ToBytes())
                    : new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToAssignAsBase, trace, result.ErrorMessage));
        }

        public delegate void MeasurementHandler(object sender, MeasurementCompletedEventArgs e);
        public delegate void BaseRefHandler(object sender, MeasurementCompletedEventArgs e);

        public event MeasurementHandler MeasurementCompleted;
        public event BaseRefHandler BaseRefAssigned;
    }
}
