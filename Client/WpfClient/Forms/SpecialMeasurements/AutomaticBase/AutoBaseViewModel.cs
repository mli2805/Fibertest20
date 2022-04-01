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
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly Model _readModel;
        private readonly AutoBaseRefLandmarksTool _autoBaseRefLandmarksTool;
        private readonly BaseRefMessages _baseRefMessages;
        private readonly ClientMeasurementModel _clientMeasurementModel;
        private readonly CurrentUser _currentUser;

        private Trace _trace;

        public bool IsOpen { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();
        public AutoParametersViewModel AutoParametersViewModel { get; set; }
        public MeasurementProgressViewModel MeasurementProgressViewModel { get; set; }

        public AutoBaseViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile, 
            IWindowManager windowManager, IWcfServiceCommonC2D c2RWcfManager,
            CurrentUser currentUser, Model readModel, 
            AutoBaseRefLandmarksTool autoBaseRefLandmarksTool, BaseRefMessages baseRefMessages)
        {
            _globalScope = globalScope;
            _iniFile = iniFile;
            _logFile = logFile;
            _windowManager = windowManager;
            _c2RWcfManager = c2RWcfManager;
            _readModel = readModel;
            _currentUser = currentUser;
            _autoBaseRefLandmarksTool = autoBaseRefLandmarksTool;
            _baseRefMessages = baseRefMessages;

            _clientMeasurementModel = new ClientMeasurementModel(currentUser, readModel);
            AutoParametersViewModel = new AutoParametersViewModel(windowManager);
        }

        public bool Initialize(TraceLeaf traceLeaf)
        {
            _trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _clientMeasurementModel.Initialize(traceLeaf, true);
            OtdrParametersViewModel.Initialize(_clientMeasurementModel.Rtu.AcceptableMeasParams, _iniFile);
            if (!AutoParametersViewModel.Initialize(_iniFile)) return false;
            MeasurementProgressViewModel = new MeasurementProgressViewModel();
            return true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Assign_base_refs_automatically;
        }

        public async void Start()
        {
            IsOpen = true;
            MeasurementProgressViewModel.ControlVisibility = Visibility.Visible;
            MeasurementProgressViewModel.IsCancelButtonEnabled = false;

            var dto = _clientMeasurementModel.PrepareDto(OtdrParametersViewModel.GetSelectedParameters(), OtdrParametersViewModel.GetVeexSelectedParameters());

            MeasurementProgressViewModel.Message = Resources.SID_Sending_command__Wait_please___;

            var startResult = await _c2RWcfManager.DoClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
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

        private async Task WaitClientMeasurementFromVeex(DoClientMeasurementDto dto, ClientMeasurementStartedDto startResult)
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = dto.RtuId,
                VeexMeasurementId = startResult.ErrorMessage, // sorry, if ReturnCode is OK, ErrorMessage contains Id
            };
            while (true)
            {
                await Task.Delay(5000);
                var measResult = await _c2RWcfManager.GetClientMeasurementAsync(getDto);

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
                    ProcessMeasurementResult(measResult.SorBytes);
                    return;
                }
            }
        }

        public async void ProcessMeasurementResult(byte[] sorBytes)
        {
            MeasurementProgressViewModel.Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            _logFile.AppendLine(@"Measurement (Client) result received");

            RftsParametersModel rftsParamsModel = AutoParametersViewModel.Model;
            var paramAutoLt = rftsParamsModel.UniParams.First(p => p.Code == @"AutoLT");
            paramAutoLt.Value = double.Parse(AutoParametersViewModel.AutoLt);
            var paramAutoRt = rftsParamsModel.UniParams.First(p => p.Code == @"AutoRT");
            paramAutoRt.Value = double.Parse(AutoParametersViewModel.AutoRt);

            var sorData = SorData.FromBytes(sorBytes);
            sorData.ApplyRftsParamsTemplate(rftsParamsModel.ToRftsParams());

            _autoBaseRefLandmarksTool.ApplyTraceToAutoBaseRef(sorData, _trace);

            BaseRefAssignedDto result;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                var dto = PrepareDto(_trace, sorData.ToBytes(), _currentUser.UserName);
                result = await _c2RWcfManager.AssignBaseRefAsync(dto); // send to Db and RTU
            }

            MeasurementProgressViewModel.ControlVisibility = Visibility.Collapsed;
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                _baseRefMessages.Display(result, _trace);
            else
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
