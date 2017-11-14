using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
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

        public MonitoringSettingsViewModel(RtuLeaf rtuLeaf, ReadModel readModel, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;

            Model = new MonitoringSettingsManager(rtuLeaf, readModel).PrepareMonitoringSettingsModel();
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_settings;
        }

        public async void Apply()
        {
            MessageProp = Resources.SID_Command_is_processing___;
            var dto = ConvertSettingsToDto();
            if (dto.IsMonitoringOn && !dto.Ports.Any())
            {
                MessageBox.Show(Resources.SID_There_are_no_ports_for_monitoring_, Resources.SID_Error_);
                return;
            }
            var resultDto = await _c2DWcfManager.ApplyMonitoringSettingsAsync(dto);
            if (resultDto.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var cmd = PrepareCommand(dto);
                var result = await _c2DWcfManager.SendCommandAsObj(cmd);
                MessageProp = result == null ? resultDto.ReturnCode.GetLocalizedString() : result;
            }
            else
                MessageProp = resultDto.ReturnCode.GetLocalizedString(resultDto.ExceptionMessage);
        }

        private ChangeMonitoringSettings PrepareCommand(ApplyMonitoringSettingsDto dto)
        {
            var cmd = new ChangeMonitoringSettings()
            {
                RtuId = dto.RtuId,
                PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                FastSave = dto.Timespans.FastSave.GetFrequency(),
                TracesInMonitoringCycle = GetTracesIncludedInMonitoringCycle(dto.Ports),
                IsMonitoringOn = dto.IsMonitoringOn,
            };
            return cmd;
        }


        private List<Guid> GetTracesIncludedInMonitoringCycle(List<OtauPortDto> ports)
        {
            var result = new List<Guid>();
            foreach (var otauPortDto in ports)
            {
                var trace = _readModel.Traces.FirstOrDefault(t => t.OtauPort != null && IsTheSamePort(t.OtauPort, otauPortDto));
                if (trace != null)
                    result.Add(trace.Id);
            }
            return result;
        }

        private bool IsTheSamePort(OtauPortDto otauPortDto1, OtauPortDto otauPortDto2)
        {
            return otauPortDto1.OtauIp == otauPortDto2.OtauIp 
                && otauPortDto1.OtauTcpPort == otauPortDto2.OtauTcpPort
                   && otauPortDto1.OpticalPort == otauPortDto2.OpticalPort;
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

        private List<OtauPortDto> ConvertPorts()
        {
            var ports = new List<OtauPortDto>();
            foreach (var charon in Model.Charons)
            {
                foreach (var port in charon.Ports.Where(p => p.IsIncluded))
                {
                    ports.Add(
                        new OtauPortDto
                        {
                            OtauIp = charon.IsMainCharon ? Model.RealOtdrAddress : charon.CharonIpAddress,
                            OtauTcpPort = charon.CharonTcpPort,
                            OpticalPort = port.PortNumber,
                            IsPortOnMainCharon = charon.IsMainCharon
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
    }
}
