using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class WholeIitRtuMeasurementsExecutor : IWholeRtuMeasurementsExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace;

        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public WholeIitRtuMeasurementsExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _dispatcherProvider = dispatcherProvider;
            _landmarksIntoBaseSetter = landmarksIntoBaseSetter;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            Model.CurrentUser = currentUser;
            Model.MeasurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            Model.OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
            Model.AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
        }

        public bool Initialize(Rtu rtu)
        {
            Model.Rtu = rtu;
            Model.TraceResults.Clear();
            Model.InterruptedPressed = false;

            Model.MeasurementProgressViewModel = new MeasurementProgressViewModel();
            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, true);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        public async Task StartOneMeasurement(RtuAutoBaseProgress item, bool keepOtdrConnection = false)
        {
            _logFile.EmptyLine();
            _logFile.AppendLine($@"Start auto base measurement for {item.Trace.Title}.");
            _trace = item.Trace;
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(item.Trace.Title);

            var dto = item.TraceLeaf.Parent
                .CreateDoClientMeasurementDto(item.TraceLeaf.PortNumber, keepOtdrConnection, _readModel, Model.CurrentUser)
                .SetParams(true, Model.AutoAnalysisParamsViewModel.SearchNewEvents, Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.StartClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
            {
                _timer.Stop();
                _timer.Dispose();

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(startResult.ReturnCode, _trace, startResult.ErrorMessage));

                return;
            }

            if (!Model.InterruptedPressed)
                _dispatcherProvider.GetDispatcher().Invoke(() =>
                {
                    Model.MeasurementProgressViewModel.Message =
                            Resources.SID_Measurement__Client__in_progress__Please_wait___;
                });

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

            MeasurementCompleted?
                  .Invoke(this, new MeasurementEventArgs(ReturnCode.MeasurementTimeoutExpired, _trace));
        }

        public void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _timer.Stop();
            _timer.Dispose();

            _logFile.AppendLine($@"Measurement (Client) result for trace {_trace.Title} received");

            if (dto.SorBytes == null)
            {
                MeasurementCompleted?.Invoke(this, new MeasurementEventArgs(dto.ReturnCode, _trace));
                return;
            }

            MeasurementCompleted?
                .Invoke(this, new MeasurementEventArgs(
                    ReturnCode.MeasurementEndedNormally, _trace, dto.SorBytes));
        }

        public async Task SetAsBaseRef(byte[] sorBytes, Trace trace)
        {
            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;

            var sorData = SorData.FromBytes(sorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner.Assign(sorData, trace);

            BaseRefAssigned?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementEventArgs(ReturnCode.BaseRefAssignedSuccessfully, trace, sorData.ToBytes())
                    : new MeasurementEventArgs(ReturnCode.BaseRefAssignmentFailed, trace, result.ErrorMessage));
        }

        public void InterruptMeasurement()
        {
            throw new System.NotImplementedException();
        }

        public delegate void MeasurementHandler(object sender, MeasurementEventArgs e);
        public delegate void BaseRefHandler(object sender, MeasurementEventArgs e);

        public event MeasurementHandler MeasurementCompleted;
        public event BaseRefHandler BaseRefAssigned;
    }
}
