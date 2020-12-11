using System.Windows.Media;
using Caliburn.Micro;

namespace Iit.Fibertest.Install
{
    public class HeaderViewModel : PropertyChangedBase
    {
        private string _inBold;
        private string _explanation;
        private Brush _fontColor = Brushes.Black;

        public string InBold
        {
            get => _inBold;
            set
            {
                if (value == _inBold) return;
                _inBold = value;
                NotifyOfPropertyChange();
            }
        }

        public string Explanation
        {
            get => _explanation;
            set
            {
                if (value == _explanation) return;
                _explanation = value;
                NotifyOfPropertyChange();
            }
        }

        public Brush FontColor
        {
            get => _fontColor;
            set
            {
                if (Equals(value, _fontColor)) return;
                _fontColor = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
