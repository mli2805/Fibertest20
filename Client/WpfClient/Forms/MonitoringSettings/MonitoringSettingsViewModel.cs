using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _desktopC2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
        private readonly IWindowManager _windowManager;
        public MonitoringSettingsModel Model { get; set; }

        public int SelectedTabIndex { get; set; }

        private string _messageProp;
        public string MessageProp
        {
            get => _messageProp;
            set
            {
                if (value == _messageProp) return;
                _messageProp = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonsEnabled;
        public bool IsButtonsEnabled
        {
            get => _isButtonsEnabled;
            set
            {
                if (value == _isButtonsEnabled) return;
                _isButtonsEnabled = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsEditEnabled));
            }
        }

        public bool IsEditEnabled => _currentUser.Role <= Role.Operator && IsButtonsEnabled;

        public MonitoringSettingsViewModel(RtuLeaf rtuLeaf, ILifetimeScope globalScope, CurrentUser currentUser, Model readModel,
            IWcfServiceDesktopC2D desktopC2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager, IWindowManager windowManager, 
            MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            _desktopC2DWcfManager = desktopC2DWcfManager;
            _commonC2DWcfManager = commonC2DWcfManager;
            _windowManager = windowManager;

            IsButtonsEnabled = true;
            Model = monitoringSettingsModelFactory.Create(rtuLeaf, IsEditEnabled);
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_settings;
        }

        public async Task Apply()
        {
            IsButtonsEnabled = false;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                MessageProp = Resources.SID_Command_is_processing___;
                var dto = ConvertSettingsToDto();
                if (dto.IsMonitoringOn && !dto.Ports.Any())
                {
                    var mb = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_are_no_ports_for_monitoring_);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                    IsButtonsEnabled = true;
                    return;
                }
                var resultDto = await _commonC2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = PrepareCommand(dto);
                    var result = await _desktopC2DWcfManager.SendCommandAsObj(cmd);
                    MessageProp = result ?? resultDto.ReturnCode.GetLocalizedString();
                }
                else
                    MessageProp = resultDto.ReturnCode.GetLocalizedString(resultDto.ErrorMessage);
            }
            IsButtonsEnabled = true;
        }

        private ChangeMonitoringSettings PrepareCommand(ApplyMonitoringSettingsDto dto)
        {
            var cmd = new ChangeMonitoringSettings()
            {
                RtuId = dto.RtuId,
                PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                FastSave = dto.Timespans.FastSave.GetFrequency(),
                TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                IsMonitoringOn = dto.IsMonitoringOn,
            };
            return cmd;
        }

        private ApplyMonitoringSettingsDto ConvertSettingsToDto()
        {
            return new ApplyMonitoringSettingsDto
            {
                RtuId = Model.RtuId,
                RtuMaker = Model.RtuMaker,
                OtdrId = Model.OtdrId,
                MainVeexOtau = Model.MainVeexOtau,
                IsMonitoringOn = Model.IsMonitoringOn,
                Timespans = ConvertFrequenciesToDto(),
                Ports = ConvertPorts()
            };
        }

        private List<PortWithTraceDto> ConvertPorts()
        {
            var ports = new List<PortWithTraceDto>();
            foreach (var charon in Model.Charons)
            {
                foreach (var port in charon.Ports.Where(p => p.IsIncluded))
                {
                    ports.Add(
                        new PortWithTraceDto()
                        {
                            OtauPort = new OtauPortDto
                            {
                                IsPortOnMainCharon = charon.IsMainCharon,
                                Serial = charon.Serial,
                                OpticalPort = port.PortNumber,
                                OtauId = charon.OtauId,
                                MainCharonPort = charon.MainCharonPort,
                            },
                            TraceId = port.TraceId,
                        });
                }
            }
            return ports;
        }

        private MonitoringTimespansDto ConvertFrequenciesToDto()
        {
            return new MonitoringTimespansDto
            {
                PreciseMeas = Model.Frequencies.SelectedPreciseMeasFreq.GetTimeSpan(),
                PreciseSave = Model.Frequencies.SelectedPreciseSaveFreq.GetTimeSpan(),
                FastSave = Model.Frequencies.SelectedFastSaveFreq.GetTimeSpan(),
            };
        }

        public async Task<int> ReSendBaseRefsForAllSelectedTraces()
        {
            MessageProp = Resources.SID_Resending_base_refs_to_RTU___;

            var ports = ConvertPorts();
            foreach (var port in ports)
            {
                var resendBaseRefDto = new ReSendBaseRefsDto()
                {
                    RtuId = Model.RtuId,
                    RtuMaker = Model.RtuMaker,
                    TraceId = port.TraceId,
                    OtauPortDto = port.OtauPort,
                    BaseRefDtos = new List<BaseRefDto>(),
                };
                foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == port.TraceId))
                {
                    resendBaseRefDto.BaseRefDtos.Add(new BaseRefDto()
                    {
                        SorFileId = baseRef.SorFileId,

                        Id = baseRef.TraceId,
                        BaseRefType = baseRef.BaseRefType,
                        Duration = baseRef.Duration,
                        SaveTimestamp = baseRef.SaveTimestamp,
                        UserName = baseRef.UserName,
                    });
                }

                var resultDto = await _commonC2DWcfManager.ReSendBaseRefAsync(resendBaseRefDto);
                if (resultDto.ReturnCode == ReturnCode.BaseRefAssignedSuccessfully)
                    MessageProp = Resources.SID_Base_refs_are_sent_to_RTU;
                else
                {
                    MessageProp = Resources.SID_Cannot_send_base_ref_to_RTU;
                    return -1;
                }
            }

            return 0;
        }

        public void Close() { TryClose(); }
    }
}
