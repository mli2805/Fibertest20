using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Iit.Fibertest.Client
{
    public class SoundManager
    {

        private void f()
        {
            MediaPlayer player = new MediaPlayer();
            var uri = new Uri(@"c:\temp\Accident.mp3", UriKind.RelativeOrAbsolute);
            player.Open(uri);
            player.Play();
        }

        private void g()
        {
            MediaPlayer player = new MediaPlayer();
            var uri = new Uri(@"c:\temp\Ok.mp3", UriKind.RelativeOrAbsolute);
            player.Open(uri);
            player.Play();
        }

        public void PlayOk()
        {
                Task.Factory.StartNew(f);
                Task.Factory.StartNew(g);
        }
    }
}