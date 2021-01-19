using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfCommonViews
{
    public class LicenseViewModel : PropertyChangedBase
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
            }
        }
    }
}
