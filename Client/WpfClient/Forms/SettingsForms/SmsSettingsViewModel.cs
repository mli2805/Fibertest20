﻿using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SmsSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public int GsmModemComPort { get; set; }
        public bool IsEditEnabled { get; set; }

        public SmsSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            GsmModemComPort = _currentDatacenterParameters.GsmModemComPort;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SMS_settings;
        }

        public async void Save()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.GsmModemComPort = GsmModemComPort;

                res = await _c2DWcfManager.SaveGsmComPort(GsmModemComPort);
            }

            if (res)
                TryClose();
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_gsm_modem_com_port_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }


        public void Cancel()
        {
            TryClose();
        }
    }
}
