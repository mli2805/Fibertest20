using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Trace = Iit.Fibertest.Graph.Trace;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly Model _readModel;
        private readonly LandmarksTool _landmarksTool;
        private readonly BaseRefMessages _baseRefMessages;
        private readonly ClientMeasurementModel _clientMeasurementModel;
        private readonly CurrentUser _currentUser;
        private readonly int _measurementTimeout;

        private Trace _trace;

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

        public AutoBaseViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile,
            IDispatcherProvider dispatcherProvider, IWindowManager windowManager, IWcfServiceCommonC2D c2DWcfCommonManager,
            CurrentUser currentUser, Model readModel,
            LandmarksTool landmarksTool, BaseRefMessages baseRefMessages)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _dispatcherProvider = dispatcherProvider;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _landmarksTool = landmarksTool;
            _baseRefMessages = baseRefMessages;

            _measurementTimeout = iniFile.Read(IniSection.Miscellaneous, IniKey.MeasurementTimeoutMs, 45000);

            _clientMeasurementModel = new ClientMeasurementModel(currentUser, readModel);
            AutoAnalysisParamsViewModel = new AutoAnalysisParamsViewModel(windowManager);
            OtdrParametersTemplatesViewModel = new OtdrParametersTemplatesViewModel(iniFile);
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _clientMeasurementModel.Initialize(traceLeaf, true);

            OtdrParametersTemplatesViewModel.Initialize(_clientMeasurementModel.Rtu);
            if (!AutoAnalysisParamsViewModel.Initialize())
                return false;
            MeasurementProgressViewModel = new MeasurementProgressViewModel();
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
            _isMeasReceived = false;
            _timer = new System.Timers.Timer(_measurementTimeout);
            _timer.Elapsed += TimeIsOver;
            _timer.AutoReset = false;
            _timer.Start();

            IsEnabled = false;
            IsOpen = true;
            MeasurementProgressViewModel.TraceTitle = _trace.Title;
            MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            var dto = _clientMeasurementModel.PrepareDto(OtdrParametersTemplatesViewModel.IsAutoLmaxSelected(),
                                             OtdrParametersTemplatesViewModel.GetSelectedParameters(),
                                                         OtdrParametersTemplatesViewModel.GetVeexSelectedParameters());

            MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;

            var startResult = await _c2DWcfCommonManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                MeasurementProgressViewModel.IsCancelButtonEnabled = false;
                IsEnabled = true;
                var vm = new MyMessageBoxViewModel(MessageType.Error, startResult.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            MeasurementProgressViewModel.IsCancelButtonEnabled = true;

            if (_clientMeasurementModel.Rtu.RtuMaker == RtuMaker.VeEX)
                await WaitClientMeasurementFromVeex(dto, startResult);
            // if RtuMaker is IIT - result will come through WCF contract
        }

        private System.Timers.Timer _timer;
        private bool _isMeasReceived;
        private void TimeIsOver(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isMeasReceived) return;

            _logFile.AppendLine(@"Measurement timeout expired");

            _dispatcherProvider.GetDispatcher().Invoke(() =>
            {
                MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error,
                    Resources.SID_Base_reference_assignment_failed));
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
                    ProcessMeasurementResult(measResultWithSorBytes.SorBytes);
                    break;
                }
            }

        }

        public async void ProcessMeasurementResult(byte[] sorBytes)
        {
            _isMeasReceived = true;
            MeasurementProgressViewModel.Message = Resources.SID_Applying_base_refs__Please_wait;
            _logFile.AppendLine(@"Measurement (Client) result received");

            var sorData = SorData.FromBytes(sorBytes);

            RftsParams rftsParams;
            if (OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id == 0)
            {
                var lmax = sorData.OwtToLenKm(sorData.FixedParameters.AcquisitionRange);
                _logFile.AppendLine($@"Fully automatic measurement: acquisition range = {lmax}");
                var index = AutoBaseParams.GetTemplateIndexByLmaxInSorData(lmax, _clientMeasurementModel.Rtu.Omid);
                _logFile.AppendLine($@"Supposedly used template #{index + 1}");
                rftsParams = AutoAnalysisParamsViewModel.LoadFromTemplate(index + 1);
            }
            else
                rftsParams = AutoAnalysisParamsViewModel.LoadFromTemplate(OtdrParametersTemplatesViewModel.Model.SelectedOtdrParametersTemplate.Id);

            rftsParams.UniParams.First(p => p.Name == @"AutoLT").Set(double.Parse(AutoAnalysisParamsViewModel.AutoLt));
            rftsParams.UniParams.First(p => p.Name == @"AutoRT").Set(double.Parse(AutoAnalysisParamsViewModel.AutoRt));
          
            sorData.ApplyRftsParamsTemplate(rftsParams);

            _landmarksTool.ApplyTraceToAutoBaseRef(sorData, _trace);

            BaseRefAssignedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = PrepareDto(_trace, sorData.ToBytes(), _currentUser.UserName);
                result = await _c2DWcfCommonManager.AssignBaseRefAsync(dto); // send to Db and RTU
            }

            MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _baseRefMessages.Display(result, _trace);
            else if (IsShowRef)
                ShowReflectogram(sorData);

            TryClose();
        }

        private void ShowReflectogram(OtdrDataKnownBlocks sorData)
        {
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(clientPath + @"\temp"))
                Directory.CreateDirectory(clientPath + @"\temp");
            var filename = clientPath + $@"\temp\meas-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.sor";
            sorData.Save(filename);
            var iitPath = FileOperations.GetParentFolder(clientPath);
            Process.Start(iitPath + @"\RftsReflect\Reflect.exe", filename);
        }

        private AssignBaseRefsDto PrepareDto(Trace trace, byte[] sorBytes, string username)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;
            var dto = new AssignBaseRefsDto()
            {
                RtuId = trace.RtuId,
                RtuMaker = rtu.RtuMaker,
                OtdrId = rtu.OtdrId,
                TraceId = trace.TraceId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = new List<BaseRefDto>(),
                DeleteOldSorFileIds = new List<int>()
            };

            if (trace.OtauPort != null && !trace.OtauPort.IsPortOnMainCharon && rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = rtu.MainVeexOtau.id,
                    OpticalPort = trace.OtauPort.MainCharonPort,
                };
            }

            dto.BaseRefs = new List<BaseRefDto>()
            {
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Precise, sorBytes, username),
                BaseRefDtoFactory.CreateFromBytes(BaseRefType.Fast, sorBytes, username)
            };
            return dto;
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
