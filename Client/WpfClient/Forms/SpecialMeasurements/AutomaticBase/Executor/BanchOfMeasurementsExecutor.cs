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
        private readonly LandmarksIntoBaseSetter _landmarksIntoBaseSetter;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;

        public OneMeasurementModel Model { get; set; } = new OneMeasurementModel();

        public BanchOfMeasurementsExecutor(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IWcfServiceCommonC2D c2DWcfCommonManager,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            LandmarksIntoBaseSetter landmarksIntoBaseSetter, MeasurementAsBaseAssigner measurementAsBaseAssigner
        )
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _landmarksIntoBaseSetter = landmarksIntoBaseSetter;
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
            Model.TraceResults.Clear();

            Model.OtdrParametersTemplatesViewModel.Initialize(rtu, true);
            return Model.AutoAnalysisParamsViewModel.Initialize();
        }

        public async Task<ClientMeasurementStartedDto> Start(RtuLeaf rtuLeaf)
        {
            Model.MeasurementProgressViewModel.DisplayStartMeasurement("");

            var dto = rtuLeaf
                .CreateDoClientMeasurementDto(_readModel, Model.CurrentUser)
                .SetParams(true, Model.OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    Model.OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    Model.OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                Model.MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;
                Model.IsEnabled = true;
                return startResult;
            }

            Model.MeasurementProgressViewModel.Message = Resources.SID_Measurements_are_in_progress__Please_wait___;
            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            // RtuMaker is IIT - result will come through WCF contract
            return startResult;
        }

        public async void ProcessMeasurementResult(ClientMeasurementResultDto dto)
        {
            // somebody detached trace while banch auto base procedure in progress :(
            if (!_readModel.TryGetTraceByOtauPortDto(dto.OtauPortDto, out Trace trace))
            {
                var message = $@"Not found trace on otau {dto.OtauPortDto.OtauId} serial {dto.OtauPortDto.Serial} port {dto.OtauPortDto.OpticalPort}";
                _logFile.AppendLine(message);
                return;
            }

            if (dto.ReturnCode != ReturnCode.MeasurementEndedNormally)
            {
                MeasurementCompletedStatus status;
                if (dto.ReturnCode == ReturnCode.InvalidValueOfLmax)
                    status = MeasurementCompletedStatus.InvalidValueLmax;
                else status = MeasurementCompletedStatus.FailedToStart;

                MeasurementCompleted?
                    .Invoke(this, new MeasurementCompletedEventArgs(status, dto, trace));
                return;
            }

            _logFile.AppendLine($@"Measurement result received for trace {trace.Title}");

            Model.MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var sorData = SorData.FromBytes(dto.SorBytes);
            var rftsParams = Model.AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, Model.OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, Model.Rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksIntoBaseSetter.ApplyTraceToAutoBaseRef(sorData, trace);
            _measurementAsBaseAssigner.Initialize(Model.Rtu);
            var result = await _measurementAsBaseAssigner
                .Assign(sorData, trace);

            MeasurementCompleted?
                .Invoke(this, result.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully
                    ? new MeasurementCompletedEventArgs(MeasurementCompletedStatus.BaseRefAssignedSuccessfully, dto, trace)
                    : new MeasurementCompletedEventArgs(MeasurementCompletedStatus.FailedToAssignAsBase, dto, trace));
        }

        public delegate void MeasurementHandler(object sender, MeasurementCompletedEventArgs e);

        public event MeasurementHandler MeasurementCompleted;
    }
}