using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly MeasurementDtoProvider _measurementDtoProvider;
        private readonly VeexMeasurementFetcher _veexMeasurementFetcher;
        private readonly MeasurementAsBaseAssigner _measurementAsBaseAssigner;
        private readonly int _measurementTimeout;
        private List<TraceLeaf> _traceLeaves;

        public bool IsOpen { get; set; }

        private Rtu _rtu;
        public OtdrParametersTemplatesViewModel OtdrParametersTemplatesViewModel { get; set; }
        public AutoAnalysisParamsViewModel AutoAnalysisParamsViewModel { get; set; }
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; }

        public bool ShouldStartMonitoring { get; set; }

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

        public RtuAutoBaseViewModel(IniFile iniFile, IMyLog logFile, Model readModel,
            IWindowManager windowManager, IWcfServiceCommonC2D c2DWcfCommonManager,
            IDispatcherProvider dispatcherProvider,
            MeasurementDtoProvider measurementDtoProvider,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel,
            VeexMeasurementFetcher veexMeasurementFetcher,
            MeasurementAsBaseAssigner measurementAsBaseAssigner
            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _dispatcherProvider = dispatcherProvider;
            _measurementDtoProvider = measurementDtoProvider;
            _veexMeasurementFetcher = veexMeasurementFetcher;
            _measurementAsBaseAssigner = measurementAsBaseAssigner;

            _measurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 60000);
            AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _traceLeaves = rtuLeaf.GetAttachedTraces();

            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);
            OtdrParametersTemplatesViewModel.Initialize(_rtu, true);
            if (!AutoAnalysisParamsViewModel.Initialize())
                return false;
            MeasurementProgressViewModel = new MeasurementProgressViewModel();
            ShouldStartMonitoring = true;
            IsEnabled = true;
            return true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            StartTimer();
            IsEnabled = false;
            IsOpen = true;
            MeasurementProgressViewModel.DisplayStartMeasurement(_traceLeaves[0].Title);

            var dto = _measurementDtoProvider
                .Initialize(_traceLeaves[0], true)
                .PrepareDto(OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                    OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                    OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                _timer.Stop();
                _timer.Dispose();
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                MeasurementProgressViewModel.IsCancelButtonEnabled = false;
                IsEnabled = true;
                var vm = new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { startResult.ReturnCode.GetLocalizedString(), startResult.ErrorMessage }, 0);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (_rtu.RtuMaker == RtuMaker.VeEX)
            {
                var veexMeasBytes = await _veexMeasurementFetcher.Fetch(dto.RtuId, startResult.ClientMeasurementId);
                if (veexMeasBytes != null)
                    ProcessMeasurementResult(veexMeasBytes);
            } // if RtuMaker is IIT - result will come through WCF contract
        }

        private System.Timers.Timer _timer;
        private void StartTimer()
        {
            _logFile.AppendLine(@"Start a measurement timeout");
            _timer = new System.Timers.Timer(_measurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();
        }
        private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        {
            _logFile.AppendLine(@"Measurement timeout expired");

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    new List<string>() { Resources.SID_Base_reference_assignment_failed, "", Resources.SID_Measurement_timeout_expired }, 0));
            });

            TryClose();
        }

        // public for WCF 
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

            Console.WriteLine($@"result {result}");
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
