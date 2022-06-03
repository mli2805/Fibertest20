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

namespace Iit.Fibertest.Client
{
    public class RtuAutoBaseViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private List<Trace> _traces;

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

        public RtuAutoBaseViewModel(IniFile iniFile, IMyLog logFile,
            Model readModel,
            IWindowManager windowManager, IWcfServiceCommonC2D c2DWcfCommonManager,
            AutoAnalysisParamsViewModel autoAnalysisParamsViewModel

            )
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;

            AutoAnalysisParamsViewModel = autoAnalysisParamsViewModel;
            OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
        }

        public bool Initialize(RtuLeaf rtuLeaf)
        {
            _traces = GetAttachedTraces(rtuLeaf).ToList();
            _traces.AddRange(rtuLeaf
                .ChildrenImpresario
                .Children
                .OfType<OtauLeaf>()
                .SelectMany(GetAttachedTraces));

            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);
            OtdrParametersTemplatesViewModel.Initialize(_rtu, true);
            if (!AutoAnalysisParamsViewModel.Initialize())
                return false;
            MeasurementProgressViewModel = new MeasurementProgressViewModel();
            ShouldStartMonitoring = true;
            IsEnabled = true;
            return true;
        }

        private IEnumerable<Trace> GetAttachedTraces(IPortOwner portOwner)
        {
            return portOwner.ChildrenImpresario
                .Children
                .OfType<TraceLeaf>()
                .Where(t => t.PortNumber > 0)
                .Select(l => _readModel.Traces.First(r => r.TraceId == l.Id))
                .ToList();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            IsEnabled = false;
            IsOpen = true;
            MeasurementProgressViewModel.TraceTitle = _traces[0].Title;
            MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;

            var dto = new DoClientMeasurementDto();
            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, startResult.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (_rtu.RtuMaker == RtuMaker.VeEX)
                await WaitClientMeasurementFromVeex(dto, startResult, _traces[0]);
            // if RtuMaker is IIT - result will come through WCF contract
        }

        private async Task WaitClientMeasurementFromVeex(DoClientMeasurementDto dto, ClientMeasurementStartedDto startResult, Trace trace)
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
                        : @"Failed to do Measurement(Client)!";

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                    {
                        firstLine,
                        "",
                        measResult.ErrorMessage,
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return;
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    var measResultWithSorBytes = await _c2DWcfCommonManager.GetClientMeasurementSorBytesAsync(getDto);
                    ProcessMeasurementResult(measResultWithSorBytes.SorBytes, trace);
                    return;
                }
            }
        }

        public void ProcessMeasurementResult(byte[] sorBytes, Trace trace)
        {
            _logFile.AppendLine($@"Measurement processing: trace {trace.Title},  {sorBytes.Length}");

            TryClose();
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
