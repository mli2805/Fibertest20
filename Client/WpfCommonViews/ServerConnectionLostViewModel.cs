using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.WpfCommonViews
{
    public class ServerConnectionLostViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        public string ServerLine { get; set; }
      
        public ServerConnectionLostViewModel(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public void Initialize(string serverTitle, string serverIp)
        {
            ServerLine = $@"{serverTitle} ({serverIp})";
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
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
