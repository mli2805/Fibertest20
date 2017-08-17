using System;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfTestBench.RtuState
{
    public class RtuStateViewModel : Screen
    {
        private readonly LogFile _logFile;
        private readonly Guid _rtuId;
        private string _currentState;

        public string CurrentState
        {
            get { return _currentState; }
            set
            {
                if (value == _currentState) return;
                _currentState = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuStateViewModel(LogFile logFile, Guid rtuId)
        {
            _logFile = logFile;
            _rtuId = rtuId;
            CurrentState = @"Unknown";
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object msg)
        {
            var dto1 = msg as RtuInitializedDto;
            if (dto1 != null)
            {
                if (dto1.RtuId == _rtuId)
                    CurrentState = @"Initialized";
                return;
            }

            var dto9 = msg as KnowRtuCurrentMonitoringStepDto;
            if (dto9 != null)
            {
                if (dto9.RtuId == _rtuId)
                    CurrentState = Dto2String(dto9);
            }

        }

        private string OtauPort2String(OtauPortDto dto)
        {
            if (dto.IsPortOnMainCharon)
                return dto.OpticalPort.ToString();
            return $@"{dto.OpticalPort} on {dto.Ip}:{dto.TcpPort}";
        }
        private string Dto2String(KnowRtuCurrentMonitoringStepDto dto)
        {
            if (dto.MonitoringStep == RtuCurrentMonitoringStep.Toggle)
                return $@"Toggling to {OtauPort2String(dto.OtauPort)}";
            if (dto.MonitoringStep == RtuCurrentMonitoringStep.Measure)
                return $@"{dto.BaseRefType} measurement port {OtauPort2String(dto.OtauPort)}";
            if (dto.MonitoringStep == RtuCurrentMonitoringStep.Analysis)
                return $@"Analysis of {dto.BaseRefType} measurement port {OtauPort2String(dto.OtauPort)}";
            return @"Idle";
        }
    }
}
