using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SmsSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public int GsmModemComPort { get; set; }

        public SmsSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            GsmModemComPort = _currentDatacenterParameters.GsmModemComPort;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SMS_settings;
        }

        public async void SaveAndSend()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.GsmModemComPort = GsmModemComPort;

                res = await _c2DWcfManager.SendTestSms(GsmModemComPort);
            }
            var message = res ? Resources.SID_Sent_successfully_ : Resources.SID_Sending_failed_;
            var vm = new MyMessageBoxViewModel(res ? MessageType.Information : MessageType.Error, message);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
