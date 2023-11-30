using System.Collections.Generic;
using Caliburn.Micro;
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
        public int GsmModemSpeed { get; set; }
        public int GsmModemTimeoutMs { get; set; }
        public bool IsEditEnabled { get; set; }

        public List<int> Speeds { get; set; } = new List<int>() { 9600, 14400, 19200, 28800, 38400, 57600, 115200 };

        public SmsSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            GsmModemComPort = _currentDatacenterParameters.Gsm.GsmModemPort;
            GsmModemSpeed = _currentDatacenterParameters.Gsm.GsmModemSpeed;
            GsmModemTimeoutMs = _currentDatacenterParameters.Gsm.GsmModemTimeoutMs;
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
                _currentDatacenterParameters.Gsm.GsmModemPort = GsmModemComPort;
                _currentDatacenterParameters.Gsm.GsmModemSpeed = GsmModemSpeed;
                _currentDatacenterParameters.Gsm.GsmModemTimeoutMs = GsmModemTimeoutMs;

                res = await _c2DWcfManager.SaveGsmSettings(_currentDatacenterParameters.Gsm);
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
