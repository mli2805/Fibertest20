using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Iit.Fibertest.Client
{
    public class OneVeexMeasurementExecutor : IOneMeasurementExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly VeexMeasurement _veexMeasurement;
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        private Trace _trace;
        public MeasurementModel Model { get; set; } = new MeasurementModel();

        public OneVeexMeasurementExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager, IDispatcherProvider dispatcherProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            VeexMeasurement veexMeasurement,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _dispatcherProvider = dispatcherProvider;
            _veexMeasurement = veexMeasurement;
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

        public async Task Start(TraceLeaf traceLeaf, bool keepOtdrConnection = false)
        {
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            StartTimer();

            Model.MeasurementProgressViewModel.DisplayStartMeasurement(traceLeaf.Title);

            VeexMeasOtdrParameters veexMeasOtdrParameters;
            if (Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected())
            {
                var lineParamsDto = await _veexMeasurement.GetLineParametersAsync(Model, traceLeaf);
                if (lineParamsDto.ReturnCode != ReturnCode.Ok)
                {
                    MeasurementCompleted?
                        .Invoke(this, new MeasurementEventArgs(lineParamsDto.ReturnCode, _trace));
                    return;
                }

                veexMeasOtdrParameters = Model.OtdrParametersTemplatesViewModel.Model
                    .GetVeexMeasOtdrParametersBase(false)
                    .FillInWithTemplate(lineParamsDto.ConnectionQuality, Model.Rtu.Omid);

                if (veexMeasOtdrParameters == null)
                {
                    MeasurementCompleted?
                        .Invoke(this, new MeasurementEventArgs(ReturnCode.InvalidValueOfLmax, _trace));

                    Model.IsEnabled = true;             
                    return;
                }
            }
            else
            {
                veexMeasOtdrParameters = Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters();
            }

            var dto = traceLeaf.Parent
                .CreateDoClientMeasurementDto(traceLeaf.PortNumber, false, _readModel, Model.CurrentUser)
                .SetParams(true, false, null, veexMeasOtdrParameters);

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
            {
                _timer.Stop();
                _timer.Dispose();
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Hidden;
                Model.IsEnabled = true;

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(startResult.ReturnCode, _trace, startResult.ErrorMessage));

                Model.IsEnabled = true;
                return;
            }

            Model.MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;

            await Task.Delay(veexMeasOtdrParameters.averagingTime == @"00:05" ? 10000 : 20000);
            var veexResult = await _veexMeasurement.Fetch(dto.RtuId, _trace, startResult.ClientMeasurementId);
            if (veexResult.Code == ReturnCode.MeasurementEndedNormally)
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
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Hidden;

                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(ReturnCode.MeasurementTimeoutExpired, _trace));
                Model.IsEnabled = true;
            });
        }

        public async void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            _timer.Stop();
            _timer.Dispose();

            if (dto.SorBytes == null)
            {
                MeasurementCompleted?
                    .Invoke(this, new MeasurementEventArgs(dto.ReturnCode, _trace));
                return;
            }

            _logFile.AppendLine(@"Measurement (Client) result received");

            Model.MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;

            var sorData = SorData.FromBytes(dto.SorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, _trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner.Assign(sorData, _trace);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementEventArgs(ReturnCode.BaseRefAssignedSuccessfully, _trace, sorData.ToBytes())
                    : new MeasurementEventArgs(ReturnCode.BaseRefAssignmentFailed, _trace, result.ErrorMessage));
        }

        public delegate void MeasurementHandler(object sender, MeasurementEventArgs e);

        public event OneMeasurementExecutor.MeasurementHandler MeasurementCompleted;
    }

}