﻿using System.Collections.Generic;
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
        private readonly LicenseCommandFactory _licenseCommandFactory;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly ILicenseFileChooser _licenseFileChooser;
        private readonly LicenseFromFileDecoder _licenseFromFileDecoder;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;

        private SecurityAdminConfirmationViewModel _vm;

        public string SecurityAdminPassword;

        public LicenseSender(IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, 
            LicenseCommandFactory licenseCommandFactory, ILicenseFileChooser licenseFileChooser,
            LicenseFromFileDecoder licenseFromFileDecoder, CurrentUser currentUser)
        {
            _licenseCommandFactory = licenseCommandFactory;
            _c2DWcfManager = c2DWcfManager;
            _licenseFileChooser = licenseFileChooser;
            _licenseFromFileDecoder = licenseFromFileDecoder;
            _windowManager = windowManager;
            _currentUser = currentUser;
        }

        public async Task<bool> ApplyLicenseFromFile(string initialDirectory = "")
        {
            var licenseInFile = _licenseFromFileDecoder.Decode(_licenseFileChooser.ChooseFilename(initialDirectory));

            if (licenseInFile == null)
               return false;

            return await ApplyLicenseFromFile(licenseInFile);
        }

        private async Task<bool> ApplyLicenseFromFile(LicenseInFile licenseInFile)
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

            SecurityAdminPassword = _vm.Password;
            return true;
        }
    }
}