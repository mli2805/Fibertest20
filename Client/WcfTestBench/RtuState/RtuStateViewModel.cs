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
            var dto = msg as RtuInitializedDto;
            if (dto != null)
            {
                if (dto.Id == _rtuId)
                    CurrentState = @"Initialized";
                return;
            }
        }
    }
}
