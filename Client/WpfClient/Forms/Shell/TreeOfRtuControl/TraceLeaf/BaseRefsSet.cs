using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class BaseRefsSet : PropertyChangedBase
    {
        private Guid _preciseId = Guid.Empty;
        private Guid _fastId = Guid.Empty;
        private Guid _additionalId = Guid.Empty;

        public Guid PreciseId
        {
            get => _preciseId;
            set
            {
                if (value.Equals(_preciseId)) return;
                _preciseId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
                NotifyOfPropertyChange(nameof(HasEnoughBaseRefsToPerformMonitoring));
            }
        }

        public TimeSpan PreciseDuration { get; set; }

        public Guid FastId
        {
            get => _fastId;
            set
            {
                if (value.Equals(_fastId)) return;
                _fastId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
                NotifyOfPropertyChange(nameof(HasEnoughBaseRefsToPerformMonitoring));
            }
        }

        public TimeSpan FastDuration { get; set; }

        public Guid AdditionalId
        {
            get => _additionalId;
            set
            {
                if (value.Equals(_additionalId)) return;
                _additionalId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasAnyBaseRef));
            }
        }

        public TimeSpan AdditionalDuration { get; set; }

        public bool HasAnyBaseRef => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
        public bool HasEnoughBaseRefsToPerformMonitoring => PreciseId != Guid.Empty && FastId != Guid.Empty;

    }
}