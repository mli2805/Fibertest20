using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LicenseSender
    {
        private readonly LicenseManager _licenseManager;
        private readonly LicenseCommandFactory _licenseCommandFactory;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;

        private SecurityAdminConfirmationViewModel _vm;

        public LicenseSender(LicenseManager licenseManager, LicenseCommandFactory licenseCommandFactory,
            IWcfServiceDesktopC2D c2DWcfManager,
            IWindowManager windowManager, CurrentUser currentUser)
        {
            _licenseManager = licenseManager;
            _licenseCommandFactory = licenseCommandFactory;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public async Task<bool> ApplyLicenseFromFile()
        {
            var licenseInFile = _licenseManager.ReadLicenseFromFileDialog();
            if (licenseInFile == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_license_file_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            return await ApplyLicenseFromFile(licenseInFile);
        }

        // from this point test can take part
        public async Task<bool> ApplyLicenseFromFile(LicenseInFile licenseInFile)
        {
            var cmd = _licenseCommandFactory.CreateFromFile(licenseInFile, _currentUser.UserId);
            if (licenseInFile.IsMachineKeyRequired)
            {
                if (!IsCorrectPasswordEntered(licenseInFile))
                    return false;
            }

            return await SendApplyLicenseCommand(cmd);
        }

        public async Task<bool> ApplyDemoLicense()
        {
            var cmd = _licenseCommandFactory.CreateDemo();
            return await SendApplyLicenseCommand(cmd);
        }

        private async Task<bool> SendApplyLicenseCommand(ApplyLicense cmd)
        {
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"ApplyLicense: " + result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_License_applied_successfully_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return true;
            }
        }

        private bool IsCorrectPasswordEntered(LicenseInFile licenseInFile)
        {
            _vm = new SecurityAdminConfirmationViewModel();
            _vm.Initialize();
            _windowManager.ShowDialogWithAssignedOwner(_vm);
            if (_vm.IsOkPressed)
            {
                var password = (string)Cryptography.Decode(licenseInFile.SecurityAdminPassword);
                if (_vm.Password != password)
                {
                    var strs = new List<string>() { @"Неверный пароль!", "", @"Лицензия не будет применена!" };
                    var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                    _windowManager.ShowDialogWithAssignedOwner(mb);
                    return false;
                }
            }
            else
            {
                var strs = new List<string>() { @"Пароль не введен.", "", @"Лицензия не будет применена!" };
                var mb = new MyMessageBoxViewModel(MessageType.Information, strs, 0);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            return true;
        }
    }
}