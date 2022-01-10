using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LicenseControlViewModel : PropertyChangedBase
    {
        private License _license;
        public License License
        {
            get => _license;
            set
            {
                if (Equals(value, _license)) return;
                _license = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsBasic));
            }
        }

        public bool IsBasic => !License.IsIncremental;
        public bool IsStandart => !License.IsMachineKeyRequired;
    }
}
