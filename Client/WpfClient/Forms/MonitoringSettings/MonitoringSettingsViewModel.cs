using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        public MonitoringSettingsModel Model { get; set; }

        public int SelectedTabIndex { get; set; }

        private string _messageProp;
        public string MessageProp
        {
            get { return _messageProp; }
            set
            {
                if (value == _messageProp) return;
                _messageProp = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsEditEnabled { get; set; }
        
        public MonitoringSettingsViewModel(RtuLeaf rtuLeaf, ILifetimeScope globalScope, CurrentUser currentUser, Model readModel, 
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, MonitoringSettingsModelFactory monitoringSettingsModelFactory)
        {
            _globalScope = globalScope;
            IsEditEnabled = currentUser.Role <= Role.Operator;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            Model = monitoringSettingsModelFactory.Create(rtuLeaf, IsEditEnabled);
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_settings;
        }

        public async void Apply()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                MessageProp = Resources.SID_Command_is_processing___;
                var dto = ConvertSettingsToDto();
                if (dto.IsMonitoringOn && !dto.Ports.Any())
                {
                    var mb = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_are_no_ports_for_monitoring_);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                    return;
                }
                var resultDto = await _c2DWcfManager.ApplyMonitoringSettingsAsync(dto);
                if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
                {
                    var cmd = PrepareCommand(dto);
                    var result = await _c2DWcfManager.SendCommandAsObj(cmd);
                    MessageProp = result ?? resultDto.ReturnCode.GetLocalizedString();
                }
                else
                    MessageProp = resultDto.ReturnCode.GetLocalizedString(resultDto.ExceptionMessage);
            }
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
//                                OtauIp = charon.IsMainCharon ? Model.RealOtdrAddress : charon.CharonIpAddress,
//                                OtauTcpPort = charon.CharonTcpPort,
                              Serial = charon.Serial,
                                OpticalPort = port.PortNumber,
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

                var resultDto = await _c2DWcfManager.ReSendBaseRefAsync(resendBaseRefDto);
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
