using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LicenseViewModel : Screen
    {
        private readonly LicenseManager _licenseManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
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

        public bool IsApplyLicenseEnabled { get; set; }

        public LicenseViewModel(Model readModel, LicenseManager licenseManager, 
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _licenseManager = licenseManager;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            License = readModel.License;
            IsApplyLicenseEnabled = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License;
        }

        public async void ApplyLicFile()
        {
            var licenseInFile = _licenseManager.ReadLicenseFromFile();
            if (licenseInFile == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_license_file_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            var cmd = new ApplyLicense()
            {
                LicenseId = licenseInFile.LicenseId,
                Owner = licenseInFile.Owner,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                Version = licenseInFile.Version,
            };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_License_applied_successfully_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
            }
        }
    }
}
