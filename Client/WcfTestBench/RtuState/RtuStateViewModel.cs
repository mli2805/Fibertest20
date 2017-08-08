using System;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;

namespace WcfTestBench.RtuState
{
    public class RtuStateViewModel : Screen
    {
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

        public RtuStateViewModel(Guid rtuId)
        {
            _rtuId = rtuId;
            CurrentState = @"Unknown";
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object msg)
        {
            var dto1 = msg as RtuInitializedDto;
            if (dto1 != null)
            {
                if (dto1.Id == _rtuId)
                    CurrentState = @"Initialized";
                return;
            }

            var dto9 = msg as KnowRtuCurrentMonitoringStepDto;
            if (dto9 != null)
            {
                if (dto9.RtuId == _rtuId)
                    CurrentState = $@"{dto9.MonitoringStep} {dto9.OtauPort}";
            }

        }
    }
}
