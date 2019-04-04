using System;
using System.Windows.Media;
using System.Windows.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WpfCommonViews
{
    public class SoundManager
    {
        private readonly IMyLog _logFile;
        private MediaPlayer _alertPlayer;
        private DispatcherTimer _alertTimer;

        private MediaPlayer _okPlayer;

        private int _alertCounter;

        public SoundManager(IMyLog logFile)
        {
            _logFile = logFile;
            InitializeAlertPlayer();
            InitializeOkPlayer();
        }

        private void InitializeAlertPlayer()
        {
            _alertPlayer = new MediaPlayer();
            var alertUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"Resources\Sounds\Accident.mp3");
            _alertPlayer.Open(alertUri);
            _alertTimer = new DispatcherTimer(TimeSpan.FromSeconds(14.2),
                DispatcherPriority.Background, (s, e) => PlayAlert(), Dispatcher.CurrentDispatcher);
            _alertTimer.IsEnabled = false;
            _alertCounter = 0;
        }

        private void InitializeOkPlayer()
        {
            _okPlayer = new MediaPlayer();
            var alertUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\Sounds\Ok.mp3");
            _okPlayer.Open(alertUri);
        }


        public void StartAlert()
        {
            _alertCounter++;
            if (_alertCounter == 1)
            {
                PlayAlert();
                _alertTimer.IsEnabled = true;
            }
        }

        public void StopAlert()
        {
            _alertCounter--;
            if (_alertCounter == 0)
            {
                _alertTimer.IsEnabled = false;
                _alertPlayer.Stop();
            }
        }

        private void PlayAlert()
        {
            _alertPlayer.Position = TimeSpan.Zero;
            _alertPlayer.Play();
        }

        public void PlayOk()
        {
            _logFile.AppendLine("PlayOk invocation");
            _okPlayer.Position = TimeSpan.Zero;
            _okPlayer.Play();
        }
    }
}