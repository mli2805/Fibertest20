using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LogOperationsViewModel : Screen
    {
        private bool _isAll = true;
        private bool _isClientStarted = true;
        private bool _isClientExited = true;
        private bool _isRtuAdded = true;
        private bool _isRtuUpdated = true;
        private bool _isRtuInitialized = true;
        private bool _isTraceAdded = true;
        private bool _isTraceUpdated = true;
        private bool _isTraceAttached = true;
        private bool _isTraceDetached = true;
        private bool _isTraceCleaned = true;
        private bool _isTraceRemoved = true;
        private bool _isBaseRefAssined = true;
        private bool _isMonitoringSettingsChanged = true;
        private bool _isMonitoringStopped = true;

        public bool IsAll
        {
            get => _isAll;
            set
            {
                if (value == _isAll) return;
                _isAll = value;
                ChangeAll();
            }
        }

        private void ChangeAll()
        {
            IsClientStarted = IsAll;
            IsClientExited = IsAll;

            IsRtuAdded = IsAll;
            IsRtuUpdated = IsAll;
            IsRtuInitialized = IsAll;

            IsTraceAdded = IsAll;
            IsTraceUpdated = IsAll;
            IsTraceAttached = IsAll;
            IsTraceDetached = IsAll;
            IsTraceCleaned = IsAll;
            IsTraceRemoved = IsAll;

            IsBaseRefAssined = IsAll;
            IsMonitoringSettingsChanged = IsAll;
            IsMonitoringStopped = IsAll;
        }

        public bool IsClientStarted
        {
            get => _isClientStarted;
            set
            {
                if (value == _isClientStarted) return;
                _isClientStarted = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsClientExited
        {
            get => _isClientExited;
            set
            {
                if (value == _isClientExited) return;
                _isClientExited = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRtuAdded
        {
            get => _isRtuAdded;
            set
            {
                if (value == _isRtuAdded) return;
                _isRtuAdded = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRtuUpdated
        {
            get => _isRtuUpdated;
            set
            {
                if (value == _isRtuUpdated) return;
                _isRtuUpdated = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRtuInitialized
        {
            get => _isRtuInitialized;
            set
            {
                if (value == _isRtuInitialized) return;
                _isRtuInitialized = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceAdded
        {
            get => _isTraceAdded;
            set
            {
                if (value == _isTraceAdded) return;
                _isTraceAdded = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceUpdated
        {
            get => _isTraceUpdated;
            set
            {
                if (value == _isTraceUpdated) return;
                _isTraceUpdated = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceAttached
        {
            get => _isTraceAttached;
            set
            {
                if (value == _isTraceAttached) return;
                _isTraceAttached = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceDetached
        {
            get => _isTraceDetached;
            set
            {
                if (value == _isTraceDetached) return;
                _isTraceDetached = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceCleaned
        {
            get => _isTraceCleaned;
            set
            {
                if (value == _isTraceCleaned) return;
                _isTraceCleaned = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsTraceRemoved
        {
            get => _isTraceRemoved;
            set
            {
                if (value == _isTraceRemoved) return;
                _isTraceRemoved = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBaseRefAssined
        {
            get => _isBaseRefAssined;
            set
            {
                if (value == _isBaseRefAssined) return;
                _isBaseRefAssined = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsMonitoringSettingsChanged
        {
            get => _isMonitoringSettingsChanged;
            set
            {
                if (value == _isMonitoringSettingsChanged) return;
                _isMonitoringSettingsChanged = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsMonitoringStopped
        {
            get => _isMonitoringStopped;
            set
            {
                if (value == _isMonitoringStopped) return;
                _isMonitoringStopped = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Choose_operations;
        }

        public void Ok() { TryClose(); }
    }
}
