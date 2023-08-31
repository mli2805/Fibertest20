using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class CurrentClientConfiguration : PropertyChangedBase
    {
        private bool _doNotSignalAboutSuspicion;
        public bool DoNotSignalAboutSuspicion
        {
            get => _doNotSignalAboutSuspicion;
            set
            {
                if (value == _doNotSignalAboutSuspicion) return;
                _doNotSignalAboutSuspicion = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _doNotSignalAboutRtuStatusEvents;

        public bool DoNotSignalAboutRtuStatusEvents
        {
            get => _doNotSignalAboutRtuStatusEvents;
            set
            {
                if (value == _doNotSignalAboutRtuStatusEvents) return;
                _doNotSignalAboutRtuStatusEvents = value;
                NotifyOfPropertyChange();
            }
        }
    }
}