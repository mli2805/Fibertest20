using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        public MonitoringSettingsModel Model { get; set; }
        public C2DWcfManager C2DWcfManager { get; set; }

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

        public MonitoringSettingsViewModel(MonitoringSettingsModel model)
        {
            Model = model;
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Monitoring_settings;
        }

        public async void Apply()
        {
            var dto = ConvertSettingsToDto();
            if (dto.IsMonitoringOn && !dto.Ports.Any())
            {
                MessageBox.Show(Resources.SID_There_are_no_ports_for_monitoring_, Resources.SID_Error_);
                return;
            }
            var transferResult = await C2DWcfManager.ApplyMonitoringSettingsAsync(dto);
            // TODO refactor to async result and show message with rtu reply, not fact of transfering command
            MessageProp = transferResult ? Resources.SID_Settings_were_transferred_successfully_ : Resources.SID_Settings_weren_t_transferred__See_logs_;
        //    MessageProp = string.Format(Resources.SID_Monitoring_started___0_, ms.IsSuccessful.ToString().ToUpper());
        //    MessageProp = string.Format(Resources.SID_Monitoring_stopped___0_, ms.IsSuccessful.ToString().ToUpper());
        //    MessageProp = string.Format(Resources.SID_Monitoring_settings_applied___0_, ms.IsSuccessful.ToString().ToUpper());
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
//                            OtauIp = charon.IsMainCharon ? _oldRtuStation.OtdrIp : charon.CharonIpAddress,
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
                PreciseMeas = Model.Frequencies.SelectedPreciseMeasFreq == Frequency.DoNot
                    ? TimeSpan.Zero
                    : TimeSpan.FromHours((int)Model.Frequencies.SelectedPreciseMeasFreq),
                PreciseSave = Model.Frequencies.SelectedPreciseSaveFreq == Frequency.DoNot
                    ? TimeSpan.Zero
                    : TimeSpan.FromHours((int)Model.Frequencies.SelectedPreciseSaveFreq),
                FastSave = Model.Frequencies.SelectedFastSaveFreq == Frequency.DoNot
                    ? TimeSpan.Zero
                    : TimeSpan.FromHours((int)Model.Frequencies.SelectedFastSaveFreq),
            };
        }
    }
}
