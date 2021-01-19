using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace LicenseViewer
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly LicenseManager _licenseManager;
        public LicenseViewModel LicenseViewModel { get; set; } = new LicenseViewModel();

        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _licenseManager = new LicenseManager();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License_viewer;
        }

        public void OpenLicFile()
        {
            var licenseInFile = _licenseManager.ReadLicenseFromFile();
            if (licenseInFile == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_license_file_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            LicenseViewModel.License = Map(licenseInFile);
        }

        private License Map(LicenseInFile licenseInFile)
        {
            return new License
            {
                Owner = licenseInFile.Owner,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                WebClientCount = new LicenseParameter(licenseInFile.WebClientCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                Version = licenseInFile.Version,
            };
        }

        public void Close()
        {
            TryClose();
        }
    }
}