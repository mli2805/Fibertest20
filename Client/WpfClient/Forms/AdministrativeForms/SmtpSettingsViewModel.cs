using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class SmtpSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string MailFrom { get; set; }
        public string MailFromPassword { get; set; }
        public int SmtpTimeoutMs { get; set; }

        public SmtpSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, 
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            SmtpHost = _currentDatacenterParameters.Smtp.SmptHost;
            SmtpPort = _currentDatacenterParameters.Smtp.SmptPort;
            MailFrom = _currentDatacenterParameters.Smtp.MailFrom;
            MailFromPassword = _currentDatacenterParameters.Smtp.MailFromPassword;
            SmtpTimeoutMs = _currentDatacenterParameters.Smtp.SmtpTimeoutMs;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_SMTP_settings;
        }

        public async void SaveAndSend()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.Smtp.SmptHost = SmtpHost;
                _currentDatacenterParameters.Smtp.SmptPort = SmtpPort;
                _currentDatacenterParameters.Smtp.MailFrom = MailFrom;
                _currentDatacenterParameters.Smtp.MailFromPassword = MailFromPassword;
                _currentDatacenterParameters.Smtp.SmtpTimeoutMs = SmtpTimeoutMs;

                var dto = new CurrentDatacenterSmtpParametersDto()
                {
                    SmptHost = SmtpHost,
                    SmptPort = SmtpPort,
                    MailFrom = MailFrom,
                    MailFromPassword = MailFromPassword,
                    SmtpTimeoutMs = SmtpTimeoutMs,
                };
                res = await _c2DWcfManager.SendTestEmail(dto);
            }
            var message = res ? Resources.SID_Sent_successfully_ : Resources.SID_Sending_failed_;
            var vm = new MyMessageBoxViewModel(res ? MessageType.Information : MessageType.Error, message);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}
