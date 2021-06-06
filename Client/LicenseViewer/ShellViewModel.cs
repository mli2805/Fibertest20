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
        public LicenseControlViewModel LicenseControlViewModel { get; set; } = new LicenseControlViewModel();

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

            LicenseControlViewModel.FromFile(licenseInFile);
        }

        public void Close()
        {
            TryClose();
        }
    }
}