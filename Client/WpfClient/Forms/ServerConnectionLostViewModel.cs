using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class ServerConnectionLostViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly SoundManager _soundManager;
        private string _serverLine;

        public string ServerLine
        {
            get => _serverLine;
            set
            {
                if (value == _serverLine) return;
                _serverLine = value;
                NotifyOfPropertyChange();
            }
        }

        public ServerConnectionLostViewModel(CurrentDatacenterParameters currentDatacenterParameters,
            SoundManager soundManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _soundManager = soundManager;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
            ServerLine = $@"{_currentDatacenterParameters.ServerTitle} ({ _currentDatacenterParameters.ServerIp})";
            _soundManager.StartAlert();
        }

        public void TurnSoundOff()
        {
            _soundManager.StopAlert();
        }

        public void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
