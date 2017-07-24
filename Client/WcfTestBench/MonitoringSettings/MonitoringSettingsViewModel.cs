﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;

namespace WcfTestBench.MonitoringSettings
{
    public class MonitoringSettingsViewModel : Screen
    {
        private readonly string _rtuIp;
        public MonitoringSettingsModel Model { get; set; }
        public WcfManager WcfManager { get; set; }

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

        public MonitoringSettingsViewModel(string rtuIp, MonitoringSettingsModel model)
        {
            _rtuIp = rtuIp;

            Model = model;
            Model.CalculateCycleTime();
            SelectedTabIndex = 0; // strange but it's necessary

            ClientWcfService.MessageReceived += MonitoringSettingsProcessServerMessage;
        }

        private void MonitoringSettingsProcessServerMessage(object msg)
        {
            var dto2 = msg as MonitoringStartedDto;
            if (dto2 != null)
            {
                ProcessMonitoringStarted(dto2);
                return;
            }

            var dto3 = msg as MonitoringStoppedDto;
            if (dto3 != null)
            {
                ProcessMonitoringStopped(dto3);
            }

            var dto4 = msg as MonitoringSettingsAppliedDto;
            if (dto4 != null)
            {
                ProcessMonitoringSettingsApplied(dto4);
            }

        }

        private void ProcessMonitoringStarted(MonitoringStartedDto ms)
        {
            MessageProp = $@"monitoring started: {ms.IsSuccessful.ToString().ToUpper()}";
        }
        private void ProcessMonitoringStopped(MonitoringStoppedDto ms)
        {
            MessageProp = $@"monitoring stopped: {ms.IsSuccessful.ToString().ToUpper()}";
        }
        private void ProcessMonitoringSettingsApplied(MonitoringSettingsAppliedDto ms)
        {
            MessageProp = $@"monitoring settings applied: {ms.IsSuccessful.ToString().ToUpper()}";
        }


        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Monitoring settings";
        }

        public void Apply()
        {
            var dto = ConvertSettingsToDto();
            var transferResult = WcfManager.ApplyMonitoringSettings(dto);
            MessageProp = transferResult ? "Command transferred successfully." : "Command wasn't transferred. See logs.";
        }

        private ApplyMonitoringSettingsDto ConvertSettingsToDto()
        {
            return new ApplyMonitoringSettingsDto
            {
                RtuIpAddress = _rtuIp,
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
                    ports.Add(new OtauPortDto { Ip = charon.CharonIpAddress, TcpPort = charon.CharonTcpPort, OpticalPort = port.PortNumber });
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
