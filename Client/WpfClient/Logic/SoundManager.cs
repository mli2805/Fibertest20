using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace Iit.Fibertest.Client
{
    public class SoundManager
    {
        private MediaPlayer AlertPlayer;
        private DispatcherTimer AlertTimer;

        private int _alertCounter;

        public SoundManager()
        {
            AlertPlayer = new MediaPlayer();
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            var alertUri = new Uri(folder + @"\Resources\Sounds\Accident.mp3");
            AlertPlayer.Open(alertUri);
            AlertTimer = new DispatcherTimer(TimeSpan.FromSeconds(15),
                DispatcherPriority.Background, (s, e) => PlayAlert(), Dispatcher.CurrentDispatcher);
            AlertTimer.IsEnabled = false;
            _alertCounter = 0;
        }



        public void StartAlert()
        {
            _alertCounter++;
            if (_alertCounter == 1)
            {
                PlayAlert();
                AlertTimer.IsEnabled = true;
            }
        }

        public void StopAlert()
        {
            _alertCounter--;
            if (_alertCounter == 0)
            {
                AlertTimer.IsEnabled = false;
                AlertPlayer.Stop();
            }
        }

        private void PlayAlert()
        {
            AlertPlayer.Position = TimeSpan.Zero;
            AlertPlayer.Play();
        }
    }
}