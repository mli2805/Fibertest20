using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class LicenseViewModel : Screen
    {
        private readonly LicenseManager _licenseManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
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

        public LicenseViewModel(Model readModel, LicenseManager licenseManager, IWcfServiceForClient c2DWcfManager)
        {
            _licenseManager = licenseManager;
            _c2DWcfManager = c2DWcfManager;
            License = readModel.License;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License;
        }

        public async void ApplyLicFile()
        {
            var license = _licenseManager.ReadLicenseFromFile();
            if (license != null)
            {
                var cmd = new ApplyLicense()
                {
                    Owner = license.Owner,
                    RtuCount = license.RtuCount,
                    ClientStationCount = license.ClientStationCount,
                    SuperClientEnabled = license.SuperClientEnabled,
                    Version = license.Version,
                };
                await _c2DWcfManager.SendCommandAsObj(cmd);
                License = license;
            }
        }
    }
}
