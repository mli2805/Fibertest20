using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class ServerConnectionLostViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly SoundManager _soundManager;
        private string _serverLine;

        public string ServerLine
        {
            get { return _serverLine; }
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
            DisplayName = Resources.SID_Error_;
            ServerLine = string.Format(Resources.SID_Cannot_restore_connection_with_server___0_____1___,
                _currentDatacenterParameters.ServerTitle, _currentDatacenterParameters.ServerIp);
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
