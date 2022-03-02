using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Install
{
    public class InstSettingsForClientViewModel : PropertyChangedBase
    {
        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isHighDensityGraph;
        public bool IsHighDensityGraph
        {
            get => _isHighDensityGraph;
            set
            {
                if (value == _isHighDensityGraph) return;
                _isHighDensityGraph = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
