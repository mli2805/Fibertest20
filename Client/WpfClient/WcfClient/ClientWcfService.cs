using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using JetBrains.Annotations;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService, INotifyPropertyChanged
    {
//        private int _cmd;
//        public int Cmd
//        {
//            get => _cmd;
//            set
//            {
//                if (value == _cmd) return;
//                _cmd = value;
//                OnPropertyChanged();
//            }
//        }

        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;

        public ClientWcfService(RtuStateViewsManager rtuStateViewsManager, ClientMeasurementViewModel clientMeasurementViewModel)
        {
            _rtuStateViewsManager = rtuStateViewsManager;
            _clientMeasurementViewModel = clientMeasurementViewModel;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto)
        {
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            return Task.FromResult(0);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
