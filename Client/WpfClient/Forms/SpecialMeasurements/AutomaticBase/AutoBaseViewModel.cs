using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Trace = Iit.Fibertest.Graph.Trace;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly Model _readModel;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;
        private readonly MeasurementDtoProvider _measurementDtoProvider;
        private readonly CurrentUser _currentUser;
        private readonly int _measurementTimeout;

        private TraceLeaf _traceLeaf;
        private Trace _trace;
        private Rtu _rtu;

        public bool IsOpen { get; set; }


        public OtdrParametersTemplatesViewModel OtdrParametersTemplatesViewModel { get; set; }
        public AutoAnalysisParamsViewModel AutoAnalysisParamsViewModel { get; set; }
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; }
        public bool IsShowRef { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
                OtdrParametersTemplatesViewModel.IsEnabled = _isEnabled;
                AutoAnalysisParamsViewModel.IsEnabled = _isEnabled;
            }
        }

        public AutoBaseViewModel(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel,
            IDispatcherProvider dispatcherProvider, IWindowManager windowManager, 
            IWcfServiceCommonC2D c2DWcfCommonManager, ReflectogramManager reflectogramManager,
            MeasurementDtoProvider measurementDtoProvider,
            MeasurementAsBaseAssigner measurementAsBaseAssigner,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel
            )
        {
            _logFile = logFile;
            _dispatcherProvider = dispatcherProvider;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _reflectogramManager = reflectogramManager;
            _readModel = readModel;
            _measurementDtoProvider = measurementDtoProvider;
            _currentUser = currentUser;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            _measurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _traceLeaf = traceLeaf;
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _rtu = _readModel.Rtus.First(r => r.Id == _trace.RtuId);

            OtdrParametersTemplatesViewModel.Initialize(_rtu, false);
            if (!AutoAnalysisParamsViewModel.Initialize())
                return false;
            MeasurementProgressViewModel = new MeasurementProgressViewModel();
            _measurementAsBaseAssigner.Initialize(_currentUser, _rtu, _trace);
            IsShowRef = true;
            IsEnabled = true;
            return true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            _logFile.AppendLine(@"Start a measurement timeout");
            _timer = new System.Timers.Timer(_measurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();

            IsEnabled = false;
            IsOpen = true;
            MeasurementProgressViewModel.TraceTitle = _trace.Title;
            MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            var dto = _measurementDtoProvider
                .Initialize(_traceLeaf, true)
                .PrepareDto(OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                                             OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                                                         OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                _timer.Stop();
                _timer.Dispose();
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                MeasurementProgressViewModel.IsCancelButtonEnabled = false;
                IsEnabled = true;
                var vm = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() {startResult.ReturnCode.GetLocalizedString(), startResult.ErrorMessage}, 0);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (_rtu.RtuMaker == RtuMaker.VeEX)
                await WaitClientMeasurementFromVeex(dto, startResult);
            // if RtuMaker is IIT - result will come through WCF contract
        }

        private System.Timers.Timer _timer;
        private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logFile.AppendLine(@"Measurement timeout expired");

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>(){ Resources.SID_Base_reference_assignment_failed, "", Resources.SID_Measurement_timeout_expired }, 0));
            });

            TryClose();
        }

        private async Task WaitClientMeasurementFromVeex(DoClientMeasurementDto dto, ClientMeasurementStartedDto startResult)
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = dto.RtuId,
                VeexMeasurementId = startResult.ClientMeasurementId.ToString(),
            };
            while (true)
            {
                await Task.Delay(5000);
                var measResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);

                if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                {
                    var firstLine = measResult.ReturnCode != ReturnCode.Ok
                        ? measResult.ReturnCode.GetLocalizedString()
                        : Resources.SID_Failed_to_do_Measurement_Client__;

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                    {
                        firstLine,
                        "",
                        measResult.ErrorMessage,
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                    IsEnabled = true;
                    break;
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    var measResultWithSorBytes = await _c2DWcfCommonManager.GetClientMeasurementSorBytesAsync(getDto);
                    _logFile.AppendLine($@"Fetched measurement {startResult.ClientMeasurementId.First6()} from VEEX RTU");
                    ProcessMeasurementResult(measResultWithSorBytes.SorBytes);
                    break;
                }
            }

        }

        public async void ProcessMeasurementResult(byte[] sorBytes)
        {
            _timer.Stop();
            _timer.Dispose();
            _logFile.AppendLine(@"Measurement (Client) result received");

            var sorData = SorData.FromBytes(sorBytes);
            var rftsParams = AutoAnalysisParamsViewModel
                .GetRftsParams(sorData, OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id, _rtu);
            sorData.ApplyRftsParamsTemplate(rftsParams);

            var result = await _measurementAsBaseAssigner
                .ProcessMeasurementResult(sorData, MeasurementProgressViewModel);

            if (result && IsShowRef)
                _reflectogramManager.ShowClientMeasurement(sorBytes);

            TryClose();
        }

        public void Close()
        {
            TryClose();
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            if (_timer != null)
                _timer.Dispose();
            callback(true);
        }
    }
}
